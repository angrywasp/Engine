using System;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Builder
{
    public class ShaderProgramBuilder
    {
        public static ShaderProgram Build(GraphicsDevice device, VertexShader vertexShader, TessellationControlShader tessCtrlShader, TessellationEvaluationShader tessEvalShader, PixelShader pixelShader)
        {
            ShaderProgram program = new ShaderProgram(device);
            string log;
            if (!program.Link(vertexShader, tessCtrlShader, tessEvalShader, pixelShader, out log))
                throw new Exception($"Shader linking failed\nVertex Shader: {vertexShader.Tag}\nPixel Shader: {pixelShader.Tag}\n{log}");

            return program;
        }
    }
}