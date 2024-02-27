using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public abstract class LightEffect : EffectBase
    {
        public EffectParameter LightColorParam;
        public EffectParameter CameraPositionParam;
        public EffectParameter GAlbedoBufferParam;
        public EffectParameter GNormalBufferParam;
        public EffectParameter GPbrBufferParam;
        public EffectParameter GDepthBufferParam;
        
        protected override void ExtractParameters(Effect effect)
        {
            LightColorParam = effect.Parameters["LightColor"];
            CameraPositionParam = effect.Parameters["CameraPosition"];
            GAlbedoBufferParam = effect.Parameters["GAlbedoBuffer"];
            GNormalBufferParam = effect.Parameters["GNormalBuffer"];
            GPbrBufferParam = effect.Parameters["GPbrBuffer"];
            GDepthBufferParam = effect.Parameters["GDepthBuffer"];
        }
    }
}
