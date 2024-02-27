using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectProgram
    {
        private Effect effect;
        private string name;
        private VertexShader vertexShader;
        private TessellationControlShader tessellationControlShader;
        private TessellationEvaluationShader tessellationEvaluationShader;
        private PixelShader pixelShader;
        private ShaderProgram shaderProgram;

        private Dictionary<string, int> locations;
        
        public VertexShader VertexShader => vertexShader;
        public TessellationControlShader TessellationControlShader => tessellationControlShader;
        public TessellationEvaluationShader TessellationEvaluationShader => tessellationEvaluationShader;
        public PixelShader PixelShader => pixelShader;
		public string Name => name;

        public ShaderProgram ShaderProgram => shaderProgram;

        public EffectProgram(Effect effect, string name, VertexShader vertexShader, TessellationControlShader tessCtrlShader, TessellationEvaluationShader tessEvalShader, PixelShader pixelShader)
        {
            this.effect = effect;
            this.name = name;
            this.vertexShader = vertexShader;
            this.tessellationControlShader = tessCtrlShader;
            this.tessellationEvaluationShader = tessEvalShader;
            this.pixelShader = pixelShader;
        }
        
        public EffectProgram(Effect effect, EffectProgram cloneSource)
        {
            this.effect = effect;
            this.name = cloneSource.Name;
            this.vertexShader = cloneSource.VertexShader;
            this.pixelShader = cloneSource.PixelShader;
        }

        public void SetData(ShaderProgram shaderProgram, Dictionary<string, int> locations)
        {
            this.shaderProgram = shaderProgram;
            this.locations = locations;
        }

        public void Apply()
        {
            var device = effect.GraphicsDevice;

            device.VertexShader = vertexShader;
            device.PixelShader = pixelShader;

            GL.UseProgram(shaderProgram.Handle);
            GraphicsExtensions.CheckGLError();

            effect.Apply();
            foreach (var l in locations)
                effect.Parameters[l.Key].Update(l.Value);

            ApplyTextures();
        }

        private void ApplyTextures()
        {
            for (int i = 0; i < pixelShader.Samplers.Count; i++)
            {
                var sampler = pixelShader.Samplers[i];
                var param = effect.Parameters[sampler.Name];

                if (param.Data == null)
                    continue;

                var texture = param.Data as Texture;

                GL.Uniform1(sampler.Location, i);
                GraphicsExtensions.CheckGLError();

                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(texture.glTarget, texture.glTexture);
                GraphicsExtensions.CheckGLError();

                sampler.State.Activate(effect.GraphicsDevice, texture.glTarget, texture.LevelCount > 1);
            }
        }
    }
}