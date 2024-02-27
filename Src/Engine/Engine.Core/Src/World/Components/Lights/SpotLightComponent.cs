using System;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Math;
using Engine.Cameras;
using Engine.Content.Model;
using Engine.Content.Model.Instance;
using Engine.Debug.Shapes;
using Engine.Graphics.Effects.Lights;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using Newtonsoft.Json;

namespace Engine.World.Components.Lights
{
    public class SpotLightComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.Lights.SpotLightComponent";

        [JsonProperty] public Degree SpotAngle { get; set; } = 45.0f;
        [JsonProperty] public float SpotExponent { get; set; } = 10.0f;
        [JsonProperty] public float Radius { get; set; } = 1.0f;
        [JsonProperty] public Vector4 Color { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public class SpotLightComponent : Component
    {
        private SpotLightComponentType _type = null;

        public new SpotLightComponentType Type => _type;

        private SpotLightEffect effect = new SpotLightEffect();
        private Degree spotAngle;
        private float spotExponent;
        private float radius;
        private Vector4 color;
        private BoundingFrustum boundingFrustum = new BoundingFrustum(Matrix4x4.Identity);
        private Matrix4x4 scaledGlobalTransform = Matrix4x4.Identity;
        private Matrix4x4 lightView;
        private Matrix4x4 lightProjection;

        private SceneGraphics graphics;

        private DebugLightCone debugCone;
        //private DebugMesh debugMesh;
        //private DebugBoundingFrustum debugBoundingFrustum;

        public Degree SpotAngle
        {
            get => spotAngle;
            set
            {
                spotAngle = value;
                UpdateLightGeometry();
            }
        }

        public float SpotExponent
        {
            get => spotExponent;
            set
            {
                spotExponent = value;
                UpdateLightGeometry();
            }
        }

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
                effect.LightColorParam?.SetValue(value);
            }
        }

        public RenderTarget2D ShadowMap { get; set; }

        public BoundingFrustum BoundingFrustum => boundingFrustum;
        public Matrix4x4 LightView => lightView;
        public Matrix4x4 LightProjection => lightProjection;

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            UpdateLightGeometry();
            graphics.AddSpotLight(this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            graphics.RemoveSpotLight(this);
            graphics.DebugRenderer.QueueShapeRemove(debugCone);
            effect.Dispose();
        }

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            this.graphics = parent.engine.Scene.Graphics;

            await effect.LoadAsync(graphics.Device).ConfigureAwait(false);
            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
            Color = _type.Color;

            spotAngle = _type.SpotAngle;
            spotExponent = _type.SpotExponent;
            radius = _type.Radius;

            debugCone = graphics.DebugRenderer.QueueShapeAdd(new DebugLightCone(0.5f, 0.5f, Microsoft.Xna.Framework.Color.Red));
            //debugMesh = graphics.DebugRenderer.QueueShapeAdd(new DebugMesh(EngineCore.Instance.Scene.Graphics.lightMesh, 1, Microsoft.Xna.Framework.Color.Blue));
            //debugBoundingFrustum = graphics.DebugRenderer.QueueShapeAdd(new DebugBoundingFrustum(boundingFrustum, Microsoft.Xna.Framework.Color.Green));
        }

        public bool ShouldDraw(Camera camera) => camera.Frustum.Intersects(boundingFrustum);

        public void SetGBuffer(GBuffer gBuffer)
        {
            effect.GAlbedoBufferParam.SetValue(gBuffer.Albedo);
            effect.GNormalBufferParam.SetValue(gBuffer.Normal);
            effect.GPbrBufferParam.SetValue(gBuffer.PBR);
            effect.GDepthBufferParam.SetValue(gBuffer.Depth);
        }

        //todo: not everything needs to be updated on every update
        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            //debugBoundingFrustum.Update(boundingFrustum);

            effect.CameraPositionParam.SetValue(camera.Position);
            effect.ViewParam.SetValue(camera.View);
            effect.ProjectionParam.SetValue(camera.Projection);
            effect.ShadowMapParam.SetValue(ShadowMap);
            UpdateLightGeometry();
        }

        private void UpdateLightGeometry()
        {
            var rad = spotAngle.ToRadians();
            var tan = MathF.Tan(rad);
            var cos = MathF.Cos(rad);
            var coneRad = radius * 2;
            var rt = coneRad * tan;

            scaledGlobalTransform = Matrix4x4.CreateScale(rt, rt, coneRad) * globalTransform.Matrix;
            Matrix4x4.Invert(scaledGlobalTransform, out lightView);
            lightProjection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, 0.001f, 0.5f);

            boundingFrustum.Matrix = lightView * lightProjection;
            debugCone.WorldMatrix = Matrix4x4.CreateRotationX(MathHelper.PiOver2) * Matrix4x4.CreateScale(rt, rt, coneRad) * globalTransform.Matrix;

            effect.WorldParam.SetValue(scaledGlobalTransform);

            effect.LightPositionParam.SetValue(scaledGlobalTransform.Translation);
            effect.InvLightRadiusSqrParam.SetValue(1.0f / (radius * radius));

            effect.LightDirParam.SetValue(Vector3.Normalize(globalTransform.Matrix.Backward()));
            effect.SpotAngleParam.SetValue(cos);
            effect.SpotExponentParam.SetValue(spotExponent / (1.0f - cos));
            effect.LightViewParam.SetValue(lightView);
            effect.LightProjectionParam.SetValue(lightProjection);
        }

        public void Draw(MeshInstance m)
        {
            effect.DefaultProgram.Apply();
            var s = m.SubMeshes[1];
            graphics.Device.DrawIndexedPrimitives(GLPrimitiveType.Triangles, s.BaseVertex, s.StartIndex, s.IndexCount);
        }
    }
}
