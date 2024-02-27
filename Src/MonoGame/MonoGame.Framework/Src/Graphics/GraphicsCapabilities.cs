// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;
using GetParamName = MonoGame.OpenGL.GetPName;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsCapabilities
    {
        public bool SupportsFramebufferObjectARB { get; private set; }
        public bool SupportsFramebufferObjectEXT { get; private set; }
        public bool SupportsFramebufferObjectIMG { get; private set; }
        public bool SupportsNonPowerOfTwo { get; private set; }
		public bool SupportsTextureFilterAnisotropic { get; private set; }
        public bool SupportsDepth24 { get; private set; }
        public bool SupportsPackedDepthStencil { get; private set; }
        public bool SupportsDepthNonLinear { get; private set; }
        public bool SupportsDxt1 { get; private set; }
        public bool SupportsS3tc { get; private set; }
        public bool SupportsPvrtc { get; private set; }
        public bool SupportsEtc1 { get; private set; }
        public bool SupportsAtitc { get; private set; }
        public bool SupportsTextureMaxLevel { get; private set; }
        public bool SupportsSRgb { get; private set; }
        public bool SupportsTextureArrays { get; private set; }
        public bool SupportsDepthClamp { get; private set; }
        public bool SupportsVertexTextures { get; private set; }
        public bool SupportsFloatTextures { get; private set; }
        public bool SupportsHalfFloatTextures { get; private set; }
        public bool SupportsNormalized { get; private set; }
        public int MaxTextureAnisotropy { get; private set; }
        public bool SupportsInstancing { get; private set; }
        public bool SupportsAdaptiveSync { get; private set; }
        internal bool SupportsBaseIndexInstancing { get; private set; }

        private int maxMultiSampleCount;

        public int MaxMultiSampleCount => maxMultiSampleCount;

        public void Initialize(GraphicsDevice device)
        {
            SupportsNonPowerOfTwo = device._maxTextureSize >= 8192;
            SupportsTextureFilterAnisotropic = GL.Extensions.Contains("GL_EXT_texture_filter_anisotropic");

            SupportsDepth24 = true;
            SupportsPackedDepthStencil = true;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;

            // Texture compression
            SupportsS3tc = GL.Extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                           GL.Extensions.Contains("GL_OES_texture_compression_S3TC") ||
                           GL.Extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                           GL.Extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || GL.Extensions.Contains("GL_EXT_texture_compression_dxt1");
            SupportsPvrtc = GL.Extensions.Contains("GL_IMG_texture_compression_pvrtc");
            SupportsEtc1 = GL.Extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
            SupportsAtitc = GL.Extensions.Contains("GL_ATI_texture_compression_atitc") ||
                            GL.Extensions.Contains("GL_AMD_compressed_ATC_texture");

            SupportsAdaptiveSync = GL.Extensions.Contains("GLX_EXT_swap_control_tear");

            // Framebuffer objects
            // if we're on GL 3.0+, frame buffer extensions are guaranteed to be present, but extensions may be missing
            // it is then safe to assume that GL_ARB_framebuffer_object is present so that the standard function are loaded
            SupportsFramebufferObjectARB = device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_framebuffer_object");
            SupportsFramebufferObjectEXT = GL.Extensions.Contains("GL_EXT_framebuffer_object");

            // Anisotropic filtering
            int anisotropy = 0;
            if (SupportsTextureFilterAnisotropic)
            {
                GL.GetInteger((GetPName)GetParamName.MaxTextureMaxAnisotropyExt, out anisotropy);
                GraphicsExtensions.CheckGLError();
            }
            MaxTextureAnisotropy = anisotropy;

            // sRGB
            SupportsSRgb = GL.Extensions.Contains("GL_EXT_texture_sRGB") && GL.Extensions.Contains("GL_EXT_framebuffer_sRGB");
            SupportsFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_texture_float"));
            SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_ARB_half_float_pixel"));;
            SupportsNormalized = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || GL.Extensions.Contains("GL_EXT_texture_norm16"));;

            // TODO: Implement OpenGL support for texture arrays
            // once we can author shaders that use texture arrays.
            SupportsTextureArrays = false;

            SupportsDepthClamp = GL.Extensions.Contains("GL_ARB_depth_clamp");

            SupportsVertexTextures = false; // For now, until we implement vertex textures in OpenGL.

            GL.GetInteger((GetPName)GetParamName.MaxSamples, out maxMultiSampleCount);

            SupportsInstancing = GL.VertexAttribDivisor != null;
            SupportsBaseIndexInstancing = GL.DrawElementsInstancedBaseInstance != null;
        }
    }
}
