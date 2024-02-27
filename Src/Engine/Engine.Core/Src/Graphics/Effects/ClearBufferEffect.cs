using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class ClearBufferEffect
    {
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        public ClearBufferEffect(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Load()
        {
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            Threading.BlockOnUIThread(() =>
            {
                var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/ClearBuffer/ClearBuffer.vert.glsl", includeDirs);
                var psG = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/ClearBuffer/ClearBuffer.gbuffer.frag.glsl", includeDirs);
                var psL = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/ClearBuffer/ClearBuffer.lbuffer.frag.glsl", includeDirs);

                effect = new EffectBuilder().Start(graphicsDevice)
                .CreateProgram(vs, psG, "ClearGBuffer")
                .CreateProgram(vs, psL, "ClearLBuffer")
                .Finish();
            });
        }

        public void Apply(int pass) => effect.Programs[pass].Apply();
    }
}
