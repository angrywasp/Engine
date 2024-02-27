using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.PostProcessing.AA.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.AA
{
    public enum Edge_Detection_Method
    {
        Color,
        Luma,
        Depth
    }

    public class SMAABackend
    {
        //pre-computed textures
        private Texture2D areaTex;
        private Texture2D searchTex;

        private RenderTarget2D renderResult;
        private RenderTarget2D edgesTex;
        private RenderTarget2D blendTex;

        private Effect effect;
        private QuadRenderer quad;
        private GraphicsDevice graphicsDevice;

        private EffectParameter colorTexParam;
        private EffectParameter depthTexParam;
        private EffectParameter edgesTexParam;
        private EffectParameter blendTexParam;
        private EffectParameter areaTexParam;
        private EffectParameter searchTexParam;
        private EffectParameter pixelSizeParam;

        private EffectProgram edgeDetectionProgram;
        private EffectProgram blendCalcProgram;
        private EffectProgram blendingProgram;

        public RenderTarget2D EdgesTex => edgesTex;
        public RenderTarget2D BlendTex => blendTex;

        public RenderTarget2D RenderResult => renderResult;

        private DepthStencilState edgeDetectionDepthStencilState;
        private DepthStencilState blendCalcDepthStencilState;
        private DepthStencilState blendDepthStencilState;

        public SMAABackend(EngineCore engine)
        {
            this.graphicsDevice = engine.GraphicsDevice;

            var basePath = "Engine/Renderer/Shaders/PostProcessing/SMAA";
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes"),
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/PostProcessing/SMAA")
            };

            Threading.BlockOnUIThread(() =>
            {
                var edgeDetectionVs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, $"{basePath}/EdgeDetection.vert.glsl", includeDirs);
                var colorEdgeDetectionPs = ContentLoader.LoadShader<PixelShader>(graphicsDevice, $"{basePath}/EdgeDetection.Color.frag.glsl", includeDirs);
                var lumaEdgeDetectionPs = ContentLoader.LoadShader<PixelShader>(graphicsDevice, $"{basePath}/EdgeDetection.Luma.frag.glsl", includeDirs);
                var depthEdgeDetectionPs = ContentLoader.LoadShader<PixelShader>(graphicsDevice, $"{basePath}/EdgeDetection.Depth.frag.glsl", includeDirs);

                var blendCalcVs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, $"{basePath}/BlendWeightCalc.vert.glsl", includeDirs);
                var blendCalcPs = ContentLoader.LoadShader<PixelShader>(graphicsDevice, $"{basePath}/BlendWeightCalc.frag.glsl", includeDirs);

                var blendingVs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, $"{basePath}/NeighbourhoodBlending.vert.glsl", includeDirs);
                var blendingPs = ContentLoader.LoadShader<PixelShader>(graphicsDevice, $"{basePath}/NeighbourhoodBlending.frag.glsl", includeDirs);

                effect = new EffectBuilder().Start(graphicsDevice)
                .CreateProgram(edgeDetectionVs, colorEdgeDetectionPs, "ColorEdgeDetection")
                .CreateProgram(edgeDetectionVs, lumaEdgeDetectionPs, "LumaEdgeDetection")
                .CreateProgram(edgeDetectionVs, depthEdgeDetectionPs, "DepthEdgeDetection")
                .CreateProgram(blendCalcVs, blendCalcPs, "BlendCalculation")
                .CreateProgram(blendingVs, blendingPs, "NeighbourhoodBlending")
                .Finish();

                edgeDetectionProgram = effect.Programs["ColorEdgeDetection"];
                blendCalcProgram = effect.Programs["BlendCalculation"];
                blendingProgram = effect.Programs["NeighbourhoodBlending"];

                quad = engine.Scene.Graphics.Quad;

                pixelSizeParam = effect.Parameters["SMAA_RT_METRICS"];
                colorTexParam = effect.Parameters["ColorTex"];
                depthTexParam = effect.Parameters["DepthTex"];
                edgesTexParam = effect.Parameters["EdgesTex"];
                areaTexParam = effect.Parameters["AreaTex"];
                searchTexParam = effect.Parameters["SearchTex"];
                blendTexParam = effect.Parameters["BlendTex"];

                LoadTextures();
            });

            edgeDetectionDepthStencilState = new DepthStencilState
            {
                StencilEnable = true,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,           
            };

            blendCalcDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilPass = StencilOperation.Keep,
                StencilFunction = CompareFunction.Equal,
                ReferenceStencil = 1
            };

            blendDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = false
            };
        }

        public void LoadTextures()
        {
            areaTex = new Texture2D(graphicsDevice, AreaTex.WIDTH, AreaTex.HEIGHT, true, SurfaceFormat.Rg);
            areaTex.SetData<byte>(AreaTex.Bytes);

            searchTex = new Texture2D(graphicsDevice, SearchTex.WIDTH, SearchTex.HEIGHT, true, SurfaceFormat.Red);
            searchTex.SetData<byte>(SearchTex.Bytes);

            areaTexParam.SetValue(areaTex);
            searchTexParam.SetValue(searchTex);
        }

        /*public Texture2D Flip(Texture2D source, bool vertical, bool horizontal)
        {
            Texture2D flipped = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            Color[] data = new Color[source.Width * source.Height];
            Color[] flippedData = new Color[data.Length];

            source.GetData(data);

            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                {
                    int idx = (horizontal ? source.Width - 1 - x : x) +((vertical ? source.Height - 1 - y : y) * source.Width);
                    flippedData[x + y * source.Width] = data[idx];
                }

                flipped.SetData(flippedData);

                return flipped;
        }*/

        public void CreateRenderTargets(int w, int h)
        {
            edgesTex = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.Depth32FStencil8);
            blendTex = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None);
            renderResult = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None);

            pixelSizeParam.SetValue(new Vector4(1.0f / w, 1.0f / h, w, h));
        }

        public void SetEdgeDetectionMethod(Edge_Detection_Method edgeDetectionMethod)
        {
            switch (edgeDetectionMethod)
            {
                case Edge_Detection_Method.Color:
                    edgeDetectionProgram = effect.Programs["ColorEdgeDetection"];
                    break;
                case Edge_Detection_Method.Luma:
                    edgeDetectionProgram = effect.Programs["LumaEdgeDetection"];
                    break;
                case Edge_Detection_Method.Depth:
                    edgeDetectionProgram = effect.Programs["DepthEdgeDetection"];
                    break;
            }
        }
        
        public void Draw(Texture2D r, Texture2D d)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.BlendState = BlendState.Opaque;

            colorTexParam.SetValue(r);
            depthTexParam.SetValue(d);

            { //Edge detection pass
                graphicsDevice.SetRenderTarget(edgesTex);
                graphicsDevice.DepthStencilState = edgeDetectionDepthStencilState;
                graphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, Vector4.Zero, 1.0f, 0);

                edgeDetectionProgram.Apply();
                quad.RenderQuad();
            }

            graphicsDevice.BlendState = BlendState.Opaque;

            { //Blend weight calculation pass
                graphicsDevice.SetRenderTarget(blendTex);
                graphicsDevice.DepthStencilState = blendCalcDepthStencilState;
                graphicsDevice.Clear(ClearOptions.Target, Vector4.Zero, 1.0f, 0);

                edgesTexParam.SetValue(edgesTex);

                blendCalcProgram.Apply();
				quad.RenderQuad();
			}

			{ //Neighbourhood blending pass
                graphicsDevice.SetRenderTarget(renderResult);
				graphicsDevice.DepthStencilState = blendDepthStencilState;
                graphicsDevice.Clear(ClearOptions.Target, Vector4.Zero, 1.0f, 0);
                
                blendTexParam.SetValue(blendTex);

				blendingProgram.Apply();
				quad.RenderQuad();
                graphicsDevice.SetRenderTarget(null);
			}
        }
    }

    public class SMAA : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/SMAA.frag.glsl";

        private SMAABackend backend;

        public SMAABackend Backend => backend;

        protected override void ExtractParameters()
        {
            base.ExtractParameters();
            
            backend = new SMAABackend(engine);
            Vector2i vs = engine.Interface.Size;
            backend.CreateRenderTargets(vs.X, vs.Y);
        }

        public override RenderTarget2D Draw(QuadRenderer quadRenderer, Texture2D previousPass)
        {
            backend.Draw(previousPass, engine.Scene.Graphics.GBuffer.Depth);
            return base.Draw(quadRenderer, backend.RenderResult);
        }

        public override void UpdateRenderTargets(int w, int h)
        {
            base.UpdateRenderTargets(w, h);
            backend.CreateRenderTargets(w, h);
        }
    }
}
