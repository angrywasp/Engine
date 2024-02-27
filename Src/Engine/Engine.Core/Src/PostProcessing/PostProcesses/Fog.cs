using Engine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class Fog : PostProcessBase
    {
        private EffectParameter fogDensityParam;
        private EffectParameter fogColorParam;
        private EffectParameter depthBufferParam;

        private float fogDensity = 0.5f;
        private Color fogColor = Color.LightGray;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Fog.frag.glsl";

        public float FogDensity
        {
            get { return fogDensity; }
            set
            {
                fogDensity = value;
                fogDensityParam.SetValue(value);
            }
        }

		public Color FogColor
		{
			get { return fogColor; }
			set
			{
				fogColor = value;
                fogColorParam.SetValue(value.ToVector3());
			}
		}

        protected override void ExtractParameters()
        {
            base.ExtractParameters();

            fogDensityParam = effect.Parameters["FogDensity"];
            fogColorParam = effect.Parameters["FogColor"];
            depthBufferParam = effect.Parameters["DepthBuffer"];

            fogDensityParam.SetValue(fogDensity);
            fogColorParam.SetValue(fogColor.ToVector3());
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            depthBufferParam.SetValue(engine.Scene.Graphics.GBuffer.Depth);
        }
    }
}
