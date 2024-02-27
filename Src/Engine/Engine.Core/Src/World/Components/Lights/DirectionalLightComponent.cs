using System;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine.Cameras;
using Engine.Graphics.Effects.Lights;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.World.Components.Lights
{
    public class DirectionalLightComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.Lights.DirectionalLightComponent";

        [JsonProperty] public float ShadowDistance { get; set; }
        [JsonProperty] public Vector3 LightDirection { get; set; }
        [JsonProperty] public Vector4 Color { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public class DirectionalLightComponent : Component
    {
        private DirectionalLightComponentType _type = null;

        public new DirectionalLightComponentType Type => _type;

        private DirectionalLightEffect effect = new DirectionalLightEffect();
        private Vector4 color;
        private Vector3 lightDirection;
        private SceneGraphics graphics;

        public float ShadowDistance { get; set; }

        public Vector3 LightDirection
        {
            get => lightDirection;
            set
            {
                lightDirection = value;
                if (value != Vector3.Zero)
                    localTransform.Update(Matrix4x4.CreateLookAt(value, Vector3.Zero, Vector3Orientation.Up));
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

        public Matrix4x4[] LightView = new Matrix4x4[ShadowConstants.NUM_CSM_SPLITS];
        public Matrix4x4[] LightProjection = new Matrix4x4[ShadowConstants.NUM_CSM_SPLITS];
        public Vector2[] ClipPlanes = new Vector2[ShadowConstants.NUM_CSM_SPLITS];
        public RenderTarget2D ShadowMap;

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            graphics.AddDirectionalLight(this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            graphics.RemoveDirectionalLight(this);
            effect.Dispose();
        }

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            this.graphics = parent.engine.Scene.Graphics;
            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
            await effect.LoadAsync(graphics.Device).ConfigureAwait(false);

            Color = _type.Color;
            ShadowDistance = _type.ShadowDistance;
            LightDirection = _type.LightDirection;
        }

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

            effect.FrustumCornersParam.SetValue(camera.FarCorners);
            effect.LightDirParam.SetValue(Vector3.Normalize(globalTransform.Matrix.Backward()));
            effect.LightViewParam.SetValue(LightView);
            effect.LightProjectionParam.SetValue(LightProjection);
            effect.ClipPlanesParam.SetValue(ClipPlanes);
            effect.CascadeDistancesParam.SetValue(new Vector3(ClipPlanes[0].X, ClipPlanes[1].X, ClipPlanes[2].X));
            effect.ShadowMapParam.SetValue(ShadowMap);
        }

        public void Draw()
        {
            effect.DefaultProgram.Apply();
            graphics.Quad.RenderQuad();
        }
    }
}
