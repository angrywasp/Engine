using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class TextureCubeEffect : WorldEffect
    {
        protected EffectParameter textureParam;

        public string TexturePath { set => EffectHelper.LoadTexture(graphicsDevice, value, textureParam); }

        public TextureCube Texture { set => textureParam.SetValue(value); }

        protected override Effect LoadEffect()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/TextureCube/TextureCube.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/TextureCube/TextureCube.frag.glsl");

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);
            textureParam = effect.Parameters["AlbedoMap"];
        }

        public void Apply(string programName = "Default") => effect.Programs[programName].Apply();
    }
}
