using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class LinearToneMapping : PostProcessBase
    {
        private float linearExposure = 1.0f;
        
        private EffectParameter linearExposureParam;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ToneMapping/Linear.frag.glsl";


        public float LinearExposure
        {
            get { return linearExposure; }
            set
            {
                linearExposure = value;
                linearExposureParam.SetValue(value);
            }
        }

        protected override void ExtractParameters()
        {
            base.ExtractParameters();

            linearExposureParam = effect.Parameters["LinearExposure"];
            linearExposureParam.SetValue(linearExposure);
        }
    }
}