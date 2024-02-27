using Engine.Interfaces;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Cameras;
using MonoGame.OpenGL;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Engine.Graphics.Vertices;
using Engine.Graphics.Materials;
using AngryWasp.Random;
using Engine.Content.Model;

namespace Engine.World.Objects
{
    public class WaterPlaneType : GameObjectType
    {
        [JsonProperty] public string WaveMap0 { get; set; }
        [JsonProperty] public string WaveMap1 { get; set; }

        [JsonProperty] public Vector2 WaveMap0Velocity { get; set; } = Vector2.UnitX * 0.001f;
        [JsonProperty] public Vector2 WaveMap1Velocity { get; set; } = Vector2.UnitY * 0.005f;

        [JsonProperty] public float WaveMapScale { get; set; } = 0.01f;
        [JsonProperty] public Vector4 WaterColor { get; set; } = new Vector4(0.5f, 0.79f, 0.75f, 0.25f);
        [JsonProperty] public Vector2i RenderTargetSize { get; set; } = new Vector2i(1024, 1024);
        [JsonProperty] public float MaximumLightExtinction { get; set; } = 0.75f;
    }

    public class WaterPlane : GameObject, IDrawableObjectForward
    {
        private WaterPlaneType _type = null;

        public new WaterPlaneType Type => _type;

        private WaterMaterial material;

        ReflectionParameters reflectionParameters;

        private Vector2 waveMap0Velocity;
        private Vector2 waveMap1Velocity;
        private Vector2 waveMap0Offset;
        private Vector2 waveMap1Offset;

        private SharedMeshData sharedMeshData;
        private int key;

        public WaterMaterial Material => material;

        private void CreateVertexIndexBuffer(GraphicsDevice graphicsDevice)
        {
            float sz = 1.0f / 2.0f;
            float height = 0;

            sharedMeshData = new SharedMeshData(graphicsDevice,
                new VertexPositionTexture[]
                {
                    new VertexPositionTexture(new Vector3(sz, height, sz), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-sz, height, sz), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3(sz, height, -sz), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(-sz, height, -sz), new Vector2(0, 0))
                },
                new int[] { 0, 1, 2, 3, 2, 1 }
            );

            key = RandomString.AlphaNumeric(8).GetHashCode();
        }

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine).ConfigureAwait(false);

            reflectionParameters = new ReflectionParameters(engine.GraphicsDevice);
            waveMap0Velocity = _type.WaveMap0Velocity;
            waveMap1Velocity = _type.WaveMap1Velocity;

            material = new WaterMaterial
            {
                WaveMap0 = _type.WaveMap0,
                WaveMap1 = _type.WaveMap1,
                WaterColor = _type.WaterColor,
                TexScale = _type.WaveMapScale,
                MaximumLightExtinction = _type.MaximumLightExtinction
            };

            await material.CreateRenderTargets(_type.RenderTargetSize).ConfigureAwait(false);
            await material.LoadAsync(engine).ConfigureAwait(false);

            CreateVertexIndexBuffer(engine.GraphicsDevice);
        }

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            engine.Scene.Graphics.AddDrawable(key, sharedMeshData, this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            engine.Scene.Graphics.RemoveDrawable(key, this);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            material.UpdateTransform(transform.Matrix, camera.View, camera.Projection);

            material.CameraPositionParam.SetValue(camera.Position);
            material.WaveMapOffset0Param.SetValue(waveMap0Offset += waveMap0Velocity * timeDelta);
            material.WaveMapOffset1Param.SetValue(waveMap1Offset += waveMap1Velocity * timeDelta);
        }

        private Matrix4x4 CreateReflectionMatrix()
        {
            Matrix4x4.Invert(transform.Matrix, out Matrix4x4 wInvTrans);
            wInvTrans = Matrix4x4.Transpose(wInvTrans);

            Plane waterPlane = new Plane(Vector4.Transform(-Vector4.UnitY, wInvTrans));
            return Matrix4x4.CreateReflection(waterPlane);
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Forward;

        public bool ShouldDraw(Camera camera) => true;

        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer) => material.DepthBufferParam.SetValue(gBuffer.Depth);

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane) { }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane) { }

        #endregion

        #region IDrawableObjectForward implementation

        public void RenderForwardPass(Camera camera)
        {
            RasterizerState rs = camera.Position.Y < transform.Translation.Y ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;
            Matrix4x4 reflectionMatrix = CreateReflectionMatrix();

            Vector4 reflectionPlane = new Vector4(Vector3.UnitY, transform.Translation.Y);

            reflectionParameters.RelfectionMatrix = reflectionMatrix;
            reflectionParameters.ClipPlane = reflectionPlane;

            engine.Scene.SetReflectionParams(name, reflectionParameters);
            
            material.CopyBuffers(reflectionParameters.RenderTarget, engine.Scene.Graphics.OutputTexture);

            material.ApplyBuffers();

            engine.GraphicsDevice.RasterizerState = rs;
            material.Apply(0);
            engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 6);
        }

        #endregion
    }
}
