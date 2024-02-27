using Engine.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class DepthBlur : PostProcessBase
    {
        private EffectParameter depthBufferParam;

        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/DepthBlur.frag.glsl";

        protected override void ExtractParameters()
        {
            base.ExtractParameters();
            depthBufferParam = effect.Parameters["DepthBuffer"];
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            depthBufferParam.SetValue(engine.Scene.Graphics.GBuffer.Depth);
        }
    }
}
