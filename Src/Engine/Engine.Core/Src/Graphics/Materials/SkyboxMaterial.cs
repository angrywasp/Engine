using Engine.Content;
using Engine.Graphics.Effects;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Numerics;

namespace Engine.Graphics.Materials
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SkyboxMaterial : MaterialBase
    {
        #region Effect Paramaters

        protected EffectParameter worldParam;
        protected EffectParameter worldViewProjectionParam;
        protected EffectParameter cameraPositionParam;
        protected EffectParameter albedoMapParam;

        #endregion

        protected MaterialClipping clipping;

        protected string albedoMap;

        [JsonProperty]
        public string AlbedoMap
        {
            get { return albedoMap; }
            set
            {
                albedoMap = value;
                if (IsLoaded)
                    EffectHelper.LoadTextureCube(engine.GraphicsDevice, value, albedoMapParam);
            }
        }

        public MaterialClipping Clipping => clipping;

        public override Effect LoadEffect()
        {
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var vertexShader = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Skybox/RenderToGBuffer.vert.glsl", includeDirs);
            var renderToGBufferPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Skybox/RenderToGBuffer.frag.glsl", includeDirs);
            var reconstructPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Skybox/Reconstruct.frag.glsl", includeDirs);

            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(vertexShader, renderToGBufferPs, "RenderToGBuffer")
            .CreateProgram(vertexShader, reconstructPs, "Reconstruct")
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            worldParam = e.Parameters["World"];
            worldViewProjectionParam = e.Parameters["WorldViewProjection"];
            cameraPositionParam = e.Parameters["CameraPosition"];

            albedoMapParam = e.Parameters["AlbedoMap"];

            clipping = new MaterialClipping(e);
        }

        public override void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            worldParam.SetValue(world);
            worldViewProjectionParam.SetValue(world * view * projection);
            cameraPositionParam.SetValue(engine.Camera.Position);
        }

        protected override void LoadResources(EngineCore engine)
        {
            EffectHelper.LoadTextureCube(engine.GraphicsDevice, albedoMap, albedoMapParam);
        }
    }
}
