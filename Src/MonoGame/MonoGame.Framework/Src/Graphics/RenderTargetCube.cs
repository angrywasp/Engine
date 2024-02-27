// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a texture cube that can be used as a render target.
    /// </summary>
    public partial class RenderTargetCube : TextureCube, IRenderTarget
    {
        public int GLTexture => glTexture;
        public TextureTarget GLTarget => glTarget;
        public int GLColorBuffer { get; set; }
        public int GLDepthBuffer { get; set; }
        public int GLStencilBuffer { get; set; }

        public DepthFormat DepthStencilFormat { get; private set; }

        public int MultiSampleCount { get; private set; }

        public RenderTargetUsage RenderTargetUsage { get; private set; }

        int IRenderTarget.Width => size;
        int IRenderTarget.Height => size;

        TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding) =>
            TextureTarget.TextureCubeMapPositiveX + renderTargetBinding.ArraySlice;

        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
            : this(graphicsDevice, size, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents)
        {            
        }

        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            : base(graphicsDevice, size, mipMap, preferredFormat, true)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = preferredMultiSampleCount;
            RenderTargetUsage = usage;

            Threading.EnsureUIThread();

            graphicsDevice.PlatformCreateRenderTarget(this, size, size, this.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                {
                    Threading.EnsureUIThread();
                    this.GraphicsDevice.PlatformDeleteRenderTarget(this);
                }
            }

            base.Dispose(disposing);
        }
    }
}
