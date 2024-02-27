using Microsoft.Xna.Framework.Graphics;

namespace Engine.Scene
{
    /*
    G-Buffer layout
    RT0: SurfaceFormat.Rgba64
       RGB: Albedo color
         A: Not used
    RT1: SurfaceFormat.Rgba64
        RG: Encoded view space normal
        BA: Encoded world normal
    RT2: SurfaceFormat.Rgba
        R: Metalness
        G: AO
        B: Roughness
        A: Displacement
    RT3: SurfaceFormat.Vector4
        R: Linear depth
        GBA: World Position
    */

    /*
    L-Buffer layout
    RT0: SurfaceFormat.Rgba64
         RGB: Light color
           A: Light intensity
    RT1: SurfaceFormat.Rgba64
         RGB: Specular color
           A: Light intensity
    */
    
    public class GBuffer
    {
        private RenderTargetBinding[] binding;
        private RenderTarget2D albedo;
        private RenderTarget2D normal;
        private RenderTarget2D pbr;
        private RenderTarget2D depth;

        public RenderTarget2D Albedo => albedo;
        public RenderTarget2D Normal => normal;
        public RenderTarget2D PBR => pbr;
        public RenderTarget2D Depth => depth;

        public RenderTargetBinding[] Binding => binding;

        public GBuffer(GraphicsDevice graphicsDevice, int w, int h)
        {
            binding = new RenderTargetBinding[]
            {
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents),
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.HalfVector3, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents),
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents),
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Vector4, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents)
            };

            albedo = (RenderTarget2D)binding[0].RenderTarget;
            normal = (RenderTarget2D)binding[1].RenderTarget;
            pbr = (RenderTarget2D)binding[2].RenderTarget;
            depth = (RenderTarget2D)binding[3].RenderTarget;
        }
    }

    public class LBuffer
    {
        private RenderTargetBinding[] binding;
        private RenderTarget2D radiance;
        private RenderTarget2D ambient;

        public RenderTarget2D Radiance => radiance;
        public RenderTarget2D Ambient => ambient;

        public RenderTargetBinding[] Binding => binding;

        public LBuffer(GraphicsDevice graphicsDevice, int w, int h)
        {
            binding = new RenderTargetBinding[]
            {
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents),
                new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.Depth32F, 0, RenderTargetUsage.DiscardContents)
            };

            radiance = (RenderTarget2D)binding[0].RenderTarget;
            ambient = (RenderTarget2D)binding[1].RenderTarget;
        }
    }
}