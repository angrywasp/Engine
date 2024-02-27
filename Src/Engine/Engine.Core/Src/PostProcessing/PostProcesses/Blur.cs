using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class Blur : PostProcessBase
    {
        private EffectParameter blurAmountParam;

        private float blurAmount = 0.001f;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Blur.frag.glsl";

        public float BlurAmount
        {
            get { return blurAmount; }
            set
            {
                blurAmount = value;
                blurAmountParam.SetValue(value);
            }
        }

        protected override void ExtractParameters()
        {
            base.ExtractParameters();

            blurAmountParam = effect.Parameters["BlurAmt"];
            blurAmountParam.SetValue(blurAmount);
        }
    }
}