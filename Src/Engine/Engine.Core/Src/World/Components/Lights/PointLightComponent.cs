using Microsoft.Xna.Framework;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Engine.Cameras;
using Engine.Debug.Shapes;
using MonoGame.OpenGL;
using Engine.Scene;
using Engine.Graphics.Effects.Lights;
using Engine.Content.Model;
using Engine.Content.Model.Instance;

namespace Engine.World.Components.Lights
{
    public class PointLightComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.Lights.PointLightComponent";

        [JsonProperty] public float Radius { get; set; } = 1.0f;
        [JsonProperty] public Vector4 Color { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public class PointLightComponent : Component
    {
        private PointLightComponentType _type = null;

        public new PointLightComponentType Type => _type;

        private PointLightEffect effect = new PointLightEffect();
        private float radius;
        private Vector4 color;
        private BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 1);
        private Matrix4x4 scaledGlobalTransform = Matrix4x4.Identity;

        private SceneGraphics graphics;

        private DebugBoundingSphere debugBoundingSphere;

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                UpdateLightGeometry();
            }
        }

        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                effect.LightColorParam.SetValue(value);
            }
        }

        public BoundingSphere BoundingSphere => boundingSphere;

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            UpdateLightGeometry();
            graphics.AddPointLight(this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            graphics.RemovePointLight(this);
            graphics.DebugRenderer.QueueShapeRemove(debugBoundingSphere);
            effect.Dispose();
        }

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            this.graphics = parent.engine.Scene.Graphics;

            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
            await effect.LoadAsync(graphics.Device).ConfigureAwait(false);
            
            Color = _type.Color;

            radius = _type.Radius;

            debugBoundingSphere = graphics.DebugRenderer.QueueShapeAdd(boundingSphere, Microsoft.Xna.Framework.Color.Green);
        }

        public bool ShouldDraw(Camera camera) => camera.Frustum.Intersects(boundingSphere);

        public void SetGBuffer(GBuffer gBuffer)
        {
            effect.GAlbedoBufferParam.SetValue(gBuffer.Albedo);
            effect.GNormalBufferParam.SetValue(gBuffer.Normal);
            effect.GPbrBufferParam.SetValue(gBuffer.PBR);
            effect.GDepthBufferParam.SetValue(gBuffer.Depth);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            
			effect.CameraPositionParam.SetValue(camera.Position);
			effect.ViewParam.SetValue(camera.View);
            effect.ProjectionParam.SetValue(camera.Projection);
            UpdateLightGeometry();
        }

        public void UpdateLightGeometry()
        {
            scaledGlobalTransform = Matrix4x4.CreateScale(radius * 2) * globalTransform.Matrix;

            boundingSphere.Radius = radius;
            boundingSphere.Center = scaledGlobalTransform.Translation;

            debugBoundingSphere.Update(boundingSphere);

            effect.WorldParam.SetValue(scaledGlobalTransform);

            effect.LightPositionParam.SetValue(scaledGlobalTransform.Translation);
            effect.InvLightRadiusSqrParam?.SetValue(1.0f / (radius * radius));
        }

        public void Draw(MeshInstance m)
        {
            effect.DefaultProgram.Apply();
            var s = m.SubMeshes[0];
            graphics.Device.DrawIndexedPrimitives(GLPrimitiveType.Triangles, s.BaseVertex, s.StartIndex, s.IndexCount);
        }
    }
}
