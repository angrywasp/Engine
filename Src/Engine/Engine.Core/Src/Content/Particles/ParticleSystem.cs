using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.OpenGL;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Cameras;
using AngryWasp.Helpers;
using Engine.Graphics.Effects;
using AngryWasp.Random;

namespace Engine.Content
{
    public class ParticleSystem
    {
        private string settingsName;
        private ParticleSettings settings;
        private EngineCore engine;
        private ParticleEffect particleEffect;
        private VertexParticle[] particles;
        private DynamicVertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int firstActiveParticle;
        private int firstNewParticle;
        private int firstFreeParticle;
        private int firstRetiredParticle;
        private float currentTime;
        private float _totalTime;
        private float accumTime;
        private int drawCounter;
        private Matrix4x4 globalTransform = Matrix4x4.Identity;
        private BoundingBox localBoundingBox = new BoundingBox(Vector3.One * -100000, Vector3.One * 100000);
        private BoundingBox globalBoundingBox = new BoundingBox(Vector3.One * -100000, Vector3.One * 100000);

        public ParticleSystem(EngineCore engine, string settingsName)
        {
            this.engine = engine;
            this.settingsName = settingsName;
        }

        public Matrix4x4 GlobalTransform
        {
            get { return globalTransform; }
            set
            {
                globalTransform = value;
                BoundingBox.Transform(ref localBoundingBox, ref globalTransform, out globalBoundingBox);
            }
        }

        public ParticleSettings Settings => settings;

        public bool Enabled { get; set; } = true;

        public BoundingBox GlobalBoundingBox => globalBoundingBox;

        public void Restart()
        {
            currentTime = _totalTime = accumTime = 0;
            drawCounter = 0;
            firstActiveParticle = firstNewParticle = firstFreeParticle = firstRetiredParticle = 0;
        }

        public async Task Initialize()
        {
            settings = ContentLoader.LoadJson<ParticleSettings>(settingsName);

            particleEffect = new ParticleEffect();
            await particleEffect.LoadAsync(engine.GraphicsDevice).ConfigureAwait(false);

            particleEffect.TexturePath = settings.TextureName;

            particleEffect.Duration = (float)settings.Duration.TotalSeconds;
            particleEffect.DurationRandomness = Settings.DurationRandomness;
            particleEffect.Gravity = Settings.Gravity;
            particleEffect.Endvelocity = Settings.EndVelocity;
            particleEffect.MinColor = Settings.MinColor;
            particleEffect.MaxColor = Settings.MaxColor;
            particleEffect.RotateSpeed = new Vector2(Settings.MinRotateSpeed, Settings.MaxRotateSpeed);
            particleEffect.StartSize = new Vector2(Settings.MinStartSize, Settings.MaxStartSize);
            particleEffect.EndSize = new Vector2(Settings.MinEndSize, Settings.MaxEndSize);

            Threading.BlockOnUIThread(() =>
            {
                // Allocate the particle array, and fill in the corner fields (which never change).
                particles = new VertexParticle[Settings.MaxParticles * 4];

                for (int i = 0; i < Settings.MaxParticles; i++)
                {
                    particles[i * 4 + 0].Corner = new Short2(-1, -1);
                    particles[i * 4 + 1].Corner = new Short2(1, -1);
                    particles[i * 4 + 2].Corner = new Short2(1, 1);
                    particles[i * 4 + 3].Corner = new Short2(-1, 1);
                }

                // Create a dynamic vertex buffer.
                vertexBuffer = new DynamicVertexBuffer(engine.GraphicsDevice, VertexParticle.VertexDeclaration, Settings.MaxParticles * 4, BufferUsage.WriteOnly);

                // Create and populate the index buffer.
                ushort[] indices = new ushort[Settings.MaxParticles * 6];

                for (int i = 0; i < Settings.MaxParticles; i++)
                {
                    indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                    indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                    indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                    indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                    indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                    indices[i * 6 + 5] = (ushort)(i * 4 + 3);
                }

                indexBuffer = new IndexBuffer(engine.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);

                indexBuffer.SetData(indices);

                //compute an approximated bounding box
                float maxVel = MathF.Max(MathF.Max(settings.MaxHorizontalVelocity, settings.MinHorizontalVelocity),
                                        MathF.Max(settings.MaxVerticalVelocity, settings.MinVerticalVelocity));
                float minVel = MathF.Min(MathF.Min(settings.MaxHorizontalVelocity, settings.MinHorizontalVelocity),
                                        MathF.Min(settings.MaxVerticalVelocity, settings.MinVerticalVelocity));
                Vector3 max = Vector3.One * (settings.MaxEndSize / 2 + (float)(System.Math.Abs(settings.Duration.TotalSeconds * maxVel)));
                Vector3 min = -Vector3.One * (settings.MaxEndSize / 2 + (float)(System.Math.Abs(settings.Duration.TotalSeconds * minVel)));

                localBoundingBox.Min = min;
                localBoundingBox.Max = max;

                BoundingBox.Transform(ref localBoundingBox, ref globalTransform, out globalBoundingBox);
            });
        }

        public bool HasActiveParticles() => firstActiveParticle != firstFreeParticle || (_totalTime < Settings.Duration.TotalSeconds || Settings.Duration.TotalSeconds < 0);

        public void Update(Camera camera, float deltaTime)
        {
            if (!Enabled)
                return;

            particleEffect.World = Matrix4x4.Identity;
            particleEffect.View = camera.View;
            particleEffect.Projection = camera.Projection;

            currentTime += deltaTime;
            _totalTime += deltaTime;
            if (settings.EmissionRate > 0)
            {
                accumTime += deltaTime;
                float emissionTime = 1.0f / Settings.EmissionRate;
                while (accumTime > emissionTime)
                {
                    AddParticle();
                    accumTime -= emissionTime;
                }
            }

            RetireActiveParticles();
            FreeRetiredParticles();

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }

        void RetireActiveParticles()
        {
            float particleDuration = (float)Settings.Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle * 4].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= Settings.MaxParticles)
                    firstActiveParticle = 0;
            }
        }

        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;
                if (age < 3)
                    break;

                firstRetiredParticle++;

                if (firstRetiredParticle >= Settings.MaxParticles)
                    firstRetiredParticle = 0;
            }
        }

        public void Draw()
        {
            if (!Enabled)
                return;

            if (vertexBuffer.IsContentLost)
                vertexBuffer.SetData(particles);

            if (firstNewParticle != firstFreeParticle)
                AddNewParticlesToVertexBuffer();

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {
                particleEffect.ViewportScale = new Vector2(0.5f / engine.GraphicsDevice.Viewport.AspectRatio, -0.5f);
                particleEffect.Time = currentTime;

                engine.GraphicsDevice.SetVertexBuffer(vertexBuffer);
                engine.GraphicsDevice.SetIndexBuffer(indexBuffer);
                particleEffect.Apply();

                if (firstActiveParticle < firstFreeParticle)
                    engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 6);
                else
                {
                    engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, firstActiveParticle * 6, (Settings.MaxParticles - firstActiveParticle) * 6);

                    if (firstFreeParticle > 0)
                        engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, firstFreeParticle * 6);
                }
            }

            drawCounter++;
        }

        void AddNewParticlesToVertexBuffer()
        {
            int stride = VertexParticle.VERTEX_SIZE;

            if (firstNewParticle < firstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (firstFreeParticle - firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (Settings.MaxParticles - firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);

                if (firstFreeParticle > 0)
                    vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }


        public void AddParticle()
        {
            Vector3 position = globalTransform.Translation;
            Vector3 velocity = new Vector3(-globalTransform.M31, -globalTransform.M32, -globalTransform.M33);
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= Settings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            //velocity *= Settings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(Settings.MinHorizontalVelocity, Settings.MaxHorizontalVelocity, engine.Random.NextFloat());

            float horizontalAngle = engine.Random.NextFloat() * MathHelper.TwoPi;// random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * MathF.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * MathF.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(Settings.MinVerticalVelocity, Settings.MaxVerticalVelocity, engine.Random.NextFloat());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color(engine.Random.NextByte(), engine.Random.NextByte(), engine.Random.NextByte(), 255);

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }
    }
}
