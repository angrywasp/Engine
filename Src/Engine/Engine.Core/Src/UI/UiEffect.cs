using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI
{
    public class UiEffect
    {
        private Effect effect;
        private GraphicsDevice graphicsDevice;
        private EffectParameter viewProjectionParam;
        private EffectParameter textureParam;

        private static readonly Matrix4x4 ViewMatrix = Matrix4x4.CreateLookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        public Matrix4x4 Projection { set => viewProjectionParam.SetValue(ViewMatrix * value); }

        public Texture2D Texture { set => textureParam.SetValue(value); }

        public void Load()
        {
            Threading.BlockOnUIThread(() =>
            {
                var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/UI/UI.vert.glsl");
                var tps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/UI/Texture.frag.glsl");
                var cps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/UI/Color.frag.glsl");

                effect = new EffectBuilder().Start(graphicsDevice)
                .CreateProgram(vs, tps, "Texture")
                .CreateProgram(vs, cps, "Color")
                .Finish();

                viewProjectionParam = effect.Parameters["ViewProjection"];
                textureParam = effect.Parameters["DiffuseMap"];

                viewProjectionParam.SetValue(Matrix4x4.Identity); 
            });
        }


        public UiEffect(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Apply(string programName) =>
            effect.Programs[programName].Apply();
    }
}
