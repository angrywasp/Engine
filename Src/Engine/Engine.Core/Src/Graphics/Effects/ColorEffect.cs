using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class ColorEffect : WorldEffect
    {
        protected EffectParameter colorParam;

        private Vector4 colorMultiplier = Color.White.ToVector4();

        public Vector4 ColorMultiplier
        {
            get { return colorMultiplier; }
            set
            {
                colorMultiplier = value;
                colorParam.SetValue(value);
            }
        }

        protected override Effect LoadEffect()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Color/Color.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Color/Color.frag.glsl");

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);

            colorParam = effect.Parameters["ColorMultiplier"];
            colorParam.SetValue(ColorMultiplier);
        }

        public void Apply() => effect.DefaultProgram.Apply();
    }
}