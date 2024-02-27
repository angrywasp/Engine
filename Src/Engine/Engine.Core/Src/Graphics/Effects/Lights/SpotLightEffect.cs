using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public class SpotLightEffect : MeshLightEffect
    {
        public EffectParameter LightDirParam;
        public EffectParameter SpotAngleParam;
        public EffectParameter SpotExponentParam;
        public EffectParameter LightViewParam;
        public EffectParameter LightProjectionParam;
        public EffectParameter ShadowMapParam;

        public EffectProgram DefaultProgram => effect.DefaultProgram;
        
        protected override Effect LoadEffect()
        {
            string[] includeDirs = {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Spot/RenderToLBuffer.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Spot/RenderToLBuffer.frag.glsl", includeDirs);

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);

            LightDirParam = effect.Parameters["LightDir"];
            SpotAngleParam = effect.Parameters["SpotAngle"];
            SpotExponentParam = effect.Parameters["SpotExponent"];
            ShadowMapParam = effect.Parameters["ShadowMap"];
            LightViewParam = effect.Parameters["LightView"];
            LightProjectionParam = effect.Parameters["LightProjection"];

            effect.Parameters["ShadowMapSize"]?.SetValue(ShadowConstants.Instance.SpotShadowMapSize);
            effect.Parameters["ShadowMapPixelSize"]?.SetValue(ShadowConstants.Instance.SpotShadowMapPixelSize);
        }
    }
}