using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class ShaderProgram
    {
        private GraphicsDevice device;
        private int handle;
        private VertexShader vertexShader;
        private PixelShader pixelShader;

        private TessellationControlShader tessellationControlShader;
        private TessellationEvaluationShader tessellationEvaluationShader;

        public int Handle => handle;

        public ShaderProgram(GraphicsDevice device)
        {
            this.device = device;
        }

        public bool Link(VertexShader vertexShader, TessellationControlShader tessCtrlShader, TessellationEvaluationShader tessEvalShader, PixelShader pixelShader, out string log)
        {
            this.vertexShader = vertexShader;
            this.tessellationControlShader = tessCtrlShader;
            this.tessellationEvaluationShader = tessEvalShader;
            this.pixelShader = pixelShader;


            int programHandle = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(programHandle, vertexShader.Handle);
            GraphicsExtensions.CheckGLError();

            if (tessCtrlShader != null)
            {
                GL.AttachShader(programHandle, tessCtrlShader.Handle);
                GraphicsExtensions.CheckGLError();
            }

            if (tessEvalShader != null)
            {
                GL.AttachShader(programHandle, tessEvalShader.Handle);
                GraphicsExtensions.CheckGLError();
            }

            GL.AttachShader(programHandle, pixelShader.Handle);
            GraphicsExtensions.CheckGLError();

            GL.LinkProgram(programHandle);
            GraphicsExtensions.CheckGLError();

            //GL.UseProgram(programHandle);
            //GraphicsExtensions.CheckGLError();

            var linked = 0;
            GL.GetProgram(programHandle, GetProgramParameterName.LinkStatus, out linked);
            GraphicsExtensions.CheckGLError();
            if (linked != (int)Bool.True)
            {
                log = GL.GetProgramInfoLog(programHandle);
            
                GL.DetachShader(programHandle, vertexShader.Handle);

                if (tessCtrlShader != null)
                    GL.DetachShader(programHandle, tessCtrlShader.Handle);

                if (tessEvalShader != null)
                    GL.DetachShader(programHandle, tessEvalShader.Handle);

                GL.DetachShader(programHandle, pixelShader.Handle);

                device.DisposeProgram(programHandle);
                return false;
            }

            this.handle = programHandle;
            log = null;
            return true;
        }

        public bool Query(ref Dictionary<string, EffectParameter> parameters, ref Dictionary<string, int> locations, string fileName)
        {
            if (vertexShader != null)
                vertexShader.ParseAttributes(handle, fileName);

            if (tessellationControlShader != null)
                tessellationControlShader.ParseAttributes(handle, fileName);

            if (tessellationEvaluationShader != null)
                tessellationEvaluationShader.ParseAttributes(handle, fileName);

            if (pixelShader != null)
                pixelShader.ParseUniforms(handle, ref parameters, ref locations);

            return true;
        }
    }
}