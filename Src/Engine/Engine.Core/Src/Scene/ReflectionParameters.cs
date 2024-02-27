using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Scene
{
    public struct ReflectionParameters
    {
        private static readonly int w = 1920;
        private static readonly int h = 1080;

        private RenderTarget2D renderTarget;
        private GBuffer gBuffer;
        private LBuffer lBuffer;

        public RenderTarget2D RenderTarget => renderTarget;

        public GBuffer GBuffer => gBuffer;

        public LBuffer LBuffer => lBuffer;

        public Matrix4x4 RelfectionMatrix { get; set; }

        public Vector4 ClipPlane { get; set; }

        public ReflectionParameters(GraphicsDevice graphicsDevice)
        {
            RelfectionMatrix = Matrix4x4.Identity;
            ClipPlane = Vector4.Zero;
            renderTarget = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents);

            gBuffer = new GBuffer(graphicsDevice, w, h);
            lBuffer = new LBuffer(graphicsDevice, w, h);
        }
    }
}
