using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public class DirectionalLightEffect : LightEffect
    {
        public EffectParameter FrustumCornersParam;
        public EffectParameter LightDirParam;
        public EffectParameter LightViewParam;
        public EffectParameter LightProjectionParam;
        public EffectParameter ClipPlanesParam;
        public EffectParameter CascadeDistancesParam;
        public EffectParameter ShadowMapParam;

        public EffectProgram DefaultProgram => effect.DefaultProgram;
        
        protected override Effect LoadEffect()
        {
            string[] includeDirs = {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Directional/RenderToLBuffer.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Directional/RenderToLBuffer.frag.glsl", includeDirs);

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);

            FrustumCornersParam = effect.Parameters["FrustumCorners"];
            LightDirParam = effect.Parameters["LightDir"];
            LightViewParam = effect.Parameters["LightView"];
            LightProjectionParam = effect.Parameters["LightProjection"];
            ClipPlanesParam = effect.Parameters["ClipPlanes"];
            CascadeDistancesParam = effect.Parameters["CascadeDistances"];
            ShadowMapParam = effect.Parameters["ShadowMap"];

            effect.Parameters["ShadowMapSize"]?.SetValue(ShadowConstants.Instance.CsmShadowMapSize);
            effect.Parameters["ShadowMapPixelSize"]?.SetValue(ShadowConstants.Instance.CsmShadowMapPixelSize);
        }
    }
}
