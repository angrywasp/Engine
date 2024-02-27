// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	public class RenderTarget2D : Texture2D, IRenderTarget
	{
        public int GLTexture => glTexture;
        public TextureTarget GLTarget => glTarget;
        public int GLColorBuffer { get; set; }
        public int GLDepthBuffer { get; set; }
        public int GLStencilBuffer { get; set; }

		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding) => glTarget;

	    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
	        : base(graphicsDevice, width, height, mipMap, preferredFormat, SurfaceType.RenderTarget)
	    {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = graphicsDevice.GetClampedMultisampleCount(preferredMultiSampleCount);
            RenderTargetUsage = usage;

            Threading.EnsureUIThread();

            graphicsDevice.PlatformCreateRenderTarget(this, width, height, this.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
	    }

		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Rgba, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}

        /// <summary>
        /// Allows child class to specify the surface type, eg: a swap chain.
        /// </summary>        
        protected RenderTarget2D(GraphicsDevice graphicsDevice,
                        int width,
                        int height,
                        bool mipMap,
                        SurfaceFormat format,
                        DepthFormat depthFormat,
                        int preferredMultiSampleCount,
                        RenderTargetUsage usage,
                        SurfaceType surfaceType)
            : base(graphicsDevice, width, height, mipMap, format, surfaceType)
        {
            DepthStencilFormat = depthFormat;
            MultiSampleCount = preferredMultiSampleCount;
            RenderTargetUsage = usage;
		}

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Threading.BlockOnUIThread(() => {
                    if (this.GraphicsDevice != null)
                        this.GraphicsDevice.PlatformDeleteRenderTarget(this);
                });
            }

            base.Dispose(disposing);
        }      
	}
}
