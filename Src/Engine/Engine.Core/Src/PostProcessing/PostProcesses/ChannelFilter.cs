using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class ChannelFilter : PostProcessBase
    {
        private EffectParameter showRedParam;
        private EffectParameter showGreenParam;
        private EffectParameter showBlueParam;
        private EffectParameter showAlphaParam;

        private bool showRed = true;
        private bool showGreen = true;
        private bool showBlue = true;
        private bool showAlpha = true;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ChannelFilter.frag.glsl";

        public bool ShowRed
        {
            get { return showRed; }
            set
            {
                showRed = value;
                showRedParam.SetValue(value);
            }
        }

        public bool ShowGreen
        {
            get { return showGreen; }
            set
            {
                showGreen = value;
                showGreenParam.SetValue(value);
            }
        }

        public bool ShowBlue
        {
            get { return showBlue; }
            set
            {
                showBlue = value;
                showBlueParam.SetValue(value);
            }
        }

        public bool ShowAlpha
        {
            get { return showAlpha; }
            set
            {
                showAlpha = value;
                showAlphaParam.SetValue(value);
            }
        }

        protected override void ExtractParameters()
        {
            base.ExtractParameters();

            showRedParam = effect.Parameters["ShowRed"];
            showGreenParam = effect.Parameters["ShowGreen"];
            showBlueParam = effect.Parameters["ShowBlue"];
            showAlphaParam = effect.Parameters["ShowAlpha"];

            showRedParam.SetValue(showRed);
            showGreenParam.SetValue(showGreen);
            showBlueParam.SetValue(showBlue);
            showAlphaParam.SetValue(showAlpha);
        }
    }
}