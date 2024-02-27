using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public class PointLightEffect : MeshLightEffect
    {
        public EffectProgram DefaultProgram => effect.DefaultProgram;

        protected override Effect LoadEffect()
        {
            string[] includeDirs = {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Point/RenderToLBuffer.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Point/RenderToLBuffer.frag.glsl", includeDirs);

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }
    }
}
