using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class ReconstructDepthEffect
    {
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        private EffectParameter gDepthBufferParam;
        private EffectParameter farClipParam;
        private EffectParameter projectionValuesParam;

        public float FarClip { set => farClipParam.SetValue(value); }
        public Vector2 ProjectionValues { set => projectionValuesParam.SetValue(value); }
        public Texture2D GDepthBuffer { set => gDepthBufferParam.SetValue(value); }

        public ReconstructDepthEffect(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Load()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/ReconstructDepth/ReconstructDepth.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/ReconstructDepth/ReconstructDepth.frag.glsl");

            effect = new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();

            gDepthBufferParam = effect.Parameters["GDepthBuffer"];
            farClipParam = effect.Parameters["FarClip"];
            projectionValuesParam = effect.Parameters["ProjectionValues"];
        }

        public void Apply() => effect.DefaultProgram.Apply();
    }
}
