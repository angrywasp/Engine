// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters
    {
        public bool HardwareModeSwitch { get; set; } = false;
        public SurfaceFormat BackBufferFormat { get; set; }
        public DepthFormat DepthStencilFormat { get; set; }
        public int BackBufferHeight { get; set; } = GraphicsDeviceManager.DefaultBackBufferHeight;
        public int BackBufferWidth { get; set; } = GraphicsDeviceManager.DefaultBackBufferWidth;
        public int MultiSampleCount { get; set; }
        public Rectangle Bounds => new Rectangle(0, 0, BackBufferWidth, BackBufferHeight);
        public bool IsFullScreen { get; set; }
        public PresentInterval PresentationInterval { get; set; }
		public RenderTargetUsage RenderTargetUsage { get; set; }

        public IntPtr WindowHandle { get; set; }

        public PresentationParameters()
        {
            Clear();
        }

        public void Clear()
        {
            BackBufferFormat = SurfaceFormat.Rgba;
            BackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
            BackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
            DepthStencilFormat = DepthFormat.None;
            MultiSampleCount = 0;
            PresentationInterval = PresentInterval.Default;
            WindowHandle = IntPtr.Zero;
        }
    }
}
