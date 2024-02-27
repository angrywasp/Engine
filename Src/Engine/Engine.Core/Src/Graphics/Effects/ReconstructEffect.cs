using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class ReconstructEffect
    {
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        private EffectParameter farClipParam;
        private EffectParameter gBufferPixelSizeParam;
        private EffectParameter projectionValuesParam;
        private EffectParameter gDepthBufferParam;
        private EffectParameter lRadianceBufferParam;
        private EffectParameter lAmbientBufferParam;

        public float FarClip { set => farClipParam?.SetValue(value); }
        public Vector2 GBufferPixelSize { set => gBufferPixelSizeParam?.SetValue(value); }
        public Vector2 ProjectionValues { set => projectionValuesParam?.SetValue(value); }
        public Texture2D GDepthBuffer { set => gDepthBufferParam?.SetValue(value); }
        public Texture2D LRadianceBuffer { set => lRadianceBufferParam?.SetValue(value); }
        public Texture2D LAmbientBuffer { set => lAmbientBufferParam?.SetValue(value); }

        public ReconstructEffect(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Load()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Reconstruct/Reconstruct.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Reconstruct/Reconstruct.frag.glsl", null);

            effect = new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();

            farClipParam = effect.Parameters["FarClip"];
            projectionValuesParam = effect.Parameters["ProjectionValues"];
            gDepthBufferParam = effect.Parameters["GDepthBuffer"];
            lRadianceBufferParam = effect.Parameters["LRadianceBuffer"];
            lAmbientBufferParam = effect.Parameters["LAmbientBuffer"];
            gBufferPixelSizeParam = effect.Parameters["GBufferPixelSize"];
        }

        public void Apply() => effect.DefaultProgram.Apply();
    }
}
