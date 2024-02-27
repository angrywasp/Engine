// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using MonoGame.OpenGL;
using PixelFormat = MonoGame.OpenGL.PixelFormat;

namespace Microsoft.Xna.Framework.Graphics
{
    public static class GraphicsExtensions
    {
#if OPENGL
        public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;

                case VertexElementFormat.Vector2:
                case VertexElementFormat.Short2:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.HalfVector2:
                    return 2;

                case VertexElementFormat.Vector3:
                    return 3;

                case VertexElementFormat.Vector4:
                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort4:
                case VertexElementFormat.HalfVector4:
                    return 4;
            }

            throw new ArgumentException();
        }

        public static VertexPointerType OpenGLVertexPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                case VertexElementFormat.HalfVector2:
                case VertexElementFormat.HalfVector4:
                    return VertexPointerType.Float;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return VertexPointerType.Short;
            }

            throw new ArgumentException();
        }

        public static VertexAttribPointerType OpenGLVertexAttribPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                    return VertexAttribPointerType.UnsignedByte;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return VertexAttribPointerType.Short;

                case VertexElementFormat.HalfVector2:
                case VertexElementFormat.HalfVector4:
                    return VertexAttribPointerType.HalfFloat;
            }

            throw new ArgumentException();
        }

        public static bool OpenGLVertexAttribNormalized(this VertexElement element)
        {
            // TODO: This may or may not be the right behavor.  
            //
            // For instance the VertexElementFormat.Byte4 format is not supposed
            // to be normalized, but this line makes it so.
            //
            // The question is in MS XNA are types normalized based on usage or
            // normalized based to their format?
            //
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;

                default:
                    return false;
            }
        }

        public static ColorPointerType OpenGLColorPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                    return ColorPointerType.Float;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                    return ColorPointerType.UnsignedByte;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                    return ColorPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return ColorPointerType.UnsignedShort;
            }

            throw new ArgumentException();
        }

        public static NormalPointerType OpenGLNormalPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                    return NormalPointerType.Float;

                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                    return NormalPointerType.Byte;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                    return NormalPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return NormalPointerType.Short;
            }

            throw new ArgumentException();
        }

        public static TexCoordPointerType OpenGLTexCoordPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                case VertexElementFormat.Vector2:
                case VertexElementFormat.Vector3:
                case VertexElementFormat.Vector4:
                case VertexElementFormat.Color:
                case VertexElementFormat.Byte4:
                    return TexCoordPointerType.Float;

                case VertexElementFormat.Short2:
                case VertexElementFormat.Short4:
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return TexCoordPointerType.Short;
            }

            throw new ArgumentException();
        }


        public static BlendEquationMode GetBlendEquationMode(this BlendFunction function)
        {
            switch (function)
            {
                case BlendFunction.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendFunction.Max:
                    return BlendEquationMode.Max;
                case BlendFunction.Min:
                    return BlendEquationMode.Min;
                case BlendFunction.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendFunction.Subtract:
                    return BlendEquationMode.FuncSubtract;

                default:
                    throw new ArgumentException();
            }
        }

        public static BlendingFactorSrc GetBlendFactorSrc(this Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return BlendingFactorSrc.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorSrc.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorSrc.One;
                case Blend.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorSrc.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFactorSrc.SrcColor;
                case Blend.Zero:
                    return BlendingFactorSrc.Zero;
                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }

        }

        public static BlendingFactorDest GetBlendFactorDest(this Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return BlendingFactorDest.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case Blend.DestinationColor:
                    return BlendingFactorDest.DstColor;
                case Blend.InverseBlendFactor:
                    return BlendingFactorDest.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case Blend.InverseDestinationColor:
                    return BlendingFactorDest.OneMinusDstColor;
                case Blend.InverseSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case Blend.InverseSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case Blend.One:
                    return BlendingFactorDest.One;
                case Blend.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case Blend.SourceAlphaSaturation:
                    return BlendingFactorDest.SrcAlphaSaturate;
                case Blend.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case Blend.Zero:
                    return BlendingFactorDest.Zero;
                default:
                    throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented.");
            }

        }

        public static DepthFunction GetDepthFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                default:
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
            }
        }

        /// <summary>
        /// Convert a <see cref="SurfaceFormat"/> to an OpenTK.Graphics.ColorFormat.
        /// This is used for setting up the backbuffer format of the OpenGL context.
        /// </summary>
        /// <returns>An OpenTK.Graphics.ColorFormat instance.</returns>
        /// <param name="format">The <see cref="SurfaceFormat"/> to convert.</param>
        internal static ColorFormat GetColorFormat(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Red:
                    return new ColorFormat(8, 0, 0, 0);
                case SurfaceFormat.Rg:
                    return new ColorFormat(8, 8, 0, 0);
                case SurfaceFormat.Rgb:
                    return new ColorFormat(8, 8, 8, 0);
                case SurfaceFormat.Rgba:
                case SurfaceFormat.Srgb:
                    return new ColorFormat(8, 8, 8, 8);
                case SurfaceFormat.Srgb64:
                case SurfaceFormat.Rgba64:
                    return new ColorFormat(16, 16, 16, 16);
                default:
                    // Floating point backbuffers formats could be implemented
                    // but they are not typically used on the backbuffer. In
                    // those cases it is better to create a render target instead.
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Converts <see cref="PresentInterval"/> to OpenGL swap interval.
        /// </summary>
        /// <returns>A value according to EXT_swap_control</returns>
        /// <param name="interval">The <see cref="PresentInterval"/> to convert.</param>
        internal static int GetSwapInterval(this PresentInterval interval)
        {
            // See http://www.opengl.org/registry/specs/EXT/swap_control.txt
            // and https://www.opengl.org/registry/specs/EXT/glx_swap_control_tear.txt
            // OpenTK checks for EXT_swap_control_tear:
            // if supported, a swap interval of -1 enables adaptive vsync;
            // otherwise -1 is converted to 1 (vsync enabled.)

            switch (interval)
            {

                case PresentInterval.Immediate:
                    return 0;
                case PresentInterval.One:
                    return 1;
                case PresentInterval.Two:
                    return 2;
                case PresentInterval.Default:
                default:
                    return -1;
            }
        }

        const SurfaceFormat InvalidFormat = (SurfaceFormat)int.MaxValue;
        internal static void GetGLFormat(this SurfaceFormat format,
            GraphicsDevice graphicsDevice,
            out PixelInternalFormat glInternalFormat,
            out PixelFormat glFormat,
            out PixelType glType)
        {
            glInternalFormat = PixelInternalFormat.Rgba;
            glFormat = PixelFormat.Rgba;
            glType = PixelType.UnsignedByte;

            var supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
            var supportsFloat = graphicsDevice.GraphicsCapabilities.SupportsFloatTextures;
            var supportsHalfFloat = graphicsDevice.GraphicsCapabilities.SupportsHalfFloatTextures;
            var supportsNormalized = graphicsDevice.GraphicsCapabilities.SupportsNormalized;

            switch (format)
            {
                case SurfaceFormat.Red:
                    glInternalFormat = PixelInternalFormat.Red;
                    glFormat = PixelFormat.Red;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Rg:
                    glInternalFormat = PixelInternalFormat.Rg;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Rgb:
                    glInternalFormat = PixelInternalFormat.Rgb;
                    glFormat = PixelFormat.Rgb;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Rgba:
                    glInternalFormat = PixelInternalFormat.Rgba;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedByte;
                    break;
                case SurfaceFormat.Srgb:
                    if (!supportsSRgb)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Srgb;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedByte;
                    break;

                case SurfaceFormat.Single:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.R32f;
                    glFormat = PixelFormat.Red;
                    glType = PixelType.Float;
                    break;
                case SurfaceFormat.Vector2:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg32f;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.Float;
                    break;
                case SurfaceFormat.Vector3:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgb32f;
                    glFormat = PixelFormat.Rgb;
                    glType = PixelType.Float;
                    break;
                case SurfaceFormat.Vector4:
                    if (!supportsFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgba32f;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.Float;
                    break;

                case SurfaceFormat.HalfSingle:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.R16f;
                    glFormat = PixelFormat.Red;
                    glType = PixelType.HalfFloat;
                    break;
                case SurfaceFormat.HalfVector2:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg16f;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.HalfFloat;
                    break;
                case SurfaceFormat.HalfVector3:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgb16f;
                    glFormat = PixelFormat.Rgb;
                    glType = PixelType.HalfFloat;
                    break;
                case SurfaceFormat.HalfVector4:
                    if (!supportsHalfFloat)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rgba16f;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.HalfFloat;
                    break;

                case SurfaceFormat.Rg32:
                    if (!supportsNormalized)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Rg16;
                    glFormat = PixelFormat.Rg;
                    glType = PixelType.UnsignedShort;
                    break;

                case SurfaceFormat.Rgba64:
                    glInternalFormat = PixelInternalFormat.Rgba16;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedShort;
                    break;
                case SurfaceFormat.Srgb64:
                    if (!supportsSRgb)
                        goto case InvalidFormat;
                    glInternalFormat = PixelInternalFormat.Srgb;
                    glFormat = PixelFormat.Rgba;
                    glType = PixelType.UnsignedShort;
                    break;
                case InvalidFormat:
                default:
                    throw new NotSupportedException(string.Format("The requested SurfaceFormat `{0}` is not supported.", format));
            }
        }

#endif // OPENGL

        public static int GetSize(this SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Red:
                    return 1;
                case SurfaceFormat.Rg:
                case SurfaceFormat.HalfSingle:
                    return 2;
                case SurfaceFormat.Rgb:
                    return 3;
                case SurfaceFormat.Rg32:
                case SurfaceFormat.Rgba:
                case SurfaceFormat.Srgb:
                case SurfaceFormat.Single:
                case SurfaceFormat.HalfVector2:
                    return 4;
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.HalfVector3:
                    return 6;
                case SurfaceFormat.Vector3:
                    return 12;
                case SurfaceFormat.Vector4:
                    return 16;
                default:
                    throw new ArgumentException();
            }
        }

        public static int GetSize(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 4;

                case VertexElementFormat.Vector2:
                    return 8;

                case VertexElementFormat.Vector3:
                    return 12;

                case VertexElementFormat.Vector4:
                    return 16;

                case VertexElementFormat.Color:
                    return 4;

                case VertexElementFormat.Byte4:
                    return 4;

                case VertexElementFormat.Short2:
                    return 4;

                case VertexElementFormat.Short4:
                    return 8;

                case VertexElementFormat.NormalizedShort2:
                    return 4;

                case VertexElementFormat.NormalizedShort4:
                    return 8;

                case VertexElementFormat.HalfVector2:
                    return 4;

                case VertexElementFormat.HalfVector4:
                    return 8;
            }
            return 0;
        }

        public static int GetBoundTexture2D()
        {
            var prevTexture = 0;
            GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
            GraphicsExtensions.LogGLError("GraphicsExtensions.GetBoundTexture2D() GL.GetInteger");
            return prevTexture;
        }

        [Conditional("DEBUG")]
        public static void CheckGLError()
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                if (!Threading.IsOnUIThread())
                    Debugger.Break();
                Debug.WriteLine("GL.GetError() returned " + error.ToString());
            }
        }

        [Conditional("DEBUG")]
        public static void LogGLError(string location)
        {
            try
            {
                GraphicsExtensions.CheckGLError();
            }
            catch (MonoGameGLException ex)
            {
                Debug.WriteLine("MonoGameGLException at " + location + " - " + ex.Message);
            }
        }
    }

    internal class MonoGameGLException : Exception
    {
        public MonoGameGLException(string message)
            : base(message)
        {
        }
    }
}
