using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Configuration;
using System.Numerics;

namespace Engine.PostProcessing.PostProcesses
{
    public class EdgeDetector : PostProcessBase
    {
        private EffectParameter halfPixelParam;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/EdgeDetector.frag.glsl";

        protected override void ExtractParameters()
        {
            base.ExtractParameters();

            halfPixelParam = effect.Parameters["halfPixel"];
            halfPixelParam.SetValue(Vector2.One / Settings.Engine.Resolution.ToVector2());
        }

        public override void UpdateRenderTargets(int w, int h)
        {
            base.UpdateRenderTargets(w, h);

            if (halfPixelParam != null)
                halfPixelParam.SetValue(Vector2.One / new Vector2(w, h));
        }
    }
}