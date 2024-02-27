using Engine.Content;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Engine.Cameras;
using Engine.Scene;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Engine.World.Components
{
    public class EmitterComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.EmitterComponent";
        
        [JsonProperty] public List<string> ParticleSettings { get; set; } = new List<string>();
    }

    public class EmitterComponent : Component
    {
        private EmitterComponentType _type = null;

        public new EmitterComponentType Type => _type;
        
        private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        private bool enabled = true;

        public List<ParticleSystem> ParticleSystems => particleSystems;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                foreach (var p in particleSystems)
                    p.Enabled = value;
            }
        }
        
        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            for (int i = 0; i < _type.ParticleSettings.Count; i++)
            {
                var ps = new ParticleSystem(parent.engine, _type.ParticleSettings[i]);
                await ps.Initialize().ConfigureAwait(false);
                particleSystems.Add(ps);
            }

            foreach (ParticleSystem p in particleSystems)
                parent.engine.Scene.Graphics.AddParticleSystem(p);

            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            
            bool hasActive = false;
            foreach (var p in particleSystems)
            {
                p.Update(camera, (float)gameTime.ElapsedGameTime.TotalSeconds);
                hasActive |= p.HasActiveParticles();
            }

            if (!hasActive)
            {
                Enabled = false;
                return;
            }

            UpdateMaterial(globalTransform.Matrix, camera, gameTime);
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Forward;

        public bool ShouldDraw(Camera camera) => true;

        public void SetLightBuffer(LBuffer lBuffer) { }

        public void UpdateMaterial(Matrix4x4 transform, Camera camera, GameTime gameTime) { }

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane) { }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane) { }

        #endregion

        public void RenderParticlesystem(Camera camera, GameTime gameTime) { }

        protected override void OnGlobalTransformChanged(WorldTransform3 globalTransform)
        {
            base.OnGlobalTransformChanged(globalTransform);
            foreach (var p in particleSystems)
                p.GlobalTransform = globalTransform.Matrix;
        }
    }
}
