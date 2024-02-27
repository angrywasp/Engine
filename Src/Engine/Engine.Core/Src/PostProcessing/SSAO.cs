using System;
using System.Numerics;
using AngryWasp.Logger;
using AngryWasp.Random;
using Engine.Cameras;
using Engine.Configuration;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class SSAO : IPostProcess
    {
        private EffectParameter RadiusParam;
        private EffectParameter FarClipParam;
        private EffectParameter BiasParam;
        private EffectParameter GBufferPixelSizeParam;
        private EffectParameter RandomTileParam;
        private EffectParameter HalfBufferHalfPixelParam;
        private EffectParameter TempBufferResParam;
        private EffectParameter SSAOResParam;
        private EffectParameter SSAOIntensityParam;
        private EffectParameter BlurDirectionParam;

        private EffectParameter SSAOBufferParam;
        private EffectParameter GNormalBufferParam;
        private EffectParameter GDepthBufferParam;
        private EffectParameter RandomBufferParam;
        
        public event EngineEventHandler<IPostProcess> Loaded;

        protected Effect effect;
		protected EngineCore engine;

        //The result of the render. Every post process needs one
        private EffectParameter renderTargetParam;
        private RenderTarget2D renderTarget;
        private RenderTarget2D halfBuffer0;
        private RenderTarget2D halfBuffer1;

        public string Name { get; set; }

        public Texture2D GenerateRandomTexture(GraphicsDevice graphicsDevice, int width = 64, int height = 64)
        {
            var count = width * height;
            var rng = new XoShiRo256PlusPlus();

            float[] data = new float[width * height * 3];
            for (int i = 0; i < data.Length; i++)
                data[i] = (float)rng.NextDouble(0, 1);

            Texture2D tex = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Vector3);
            tex.SetData<float>(data);

            //var t2d = td.ToTextureData2D();
            //TextureHelper.SaveBitmap(t2d, EngineFolders.ContentPathVirtualToReal("ssaoRandom.png"));
            return tex;
        }

        public void Init(EngineCore engine)
        {
            this.engine = engine;

            var basePath = "Engine/Renderer/Shaders/PostProcessing/SSAO";
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            Threading.BlockOnUIThread(() =>
            {
                try
                {
                    var vsSsao = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"{basePath}/SSAO.vert.glsl");
                    var psSsao = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"{basePath}/SSAO.frag.glsl", includeDirs);
                    var vsSsaoBlur = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"{basePath}/SSAO.blur.vert.glsl");
                    var psSsaoBlur = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"{basePath}/SSAO.blur.frag.glsl", includeDirs);
                    var vsSsaoFinal = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"{basePath}/SSAO.final.vert.glsl");
                    var psSsaoFinal = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"{basePath}/SSAO.final.frag.glsl", includeDirs);

                    effect = new EffectBuilder().Start(engine.GraphicsDevice)
                    .CreateProgram(vsSsao, psSsao, "ssao")
                    .CreateProgram(vsSsaoBlur, psSsaoBlur, "ssaoBlur")
                    .CreateProgram(vsSsaoFinal, psSsaoFinal, "ssaoFinal")
                    .Finish();

                    ExtractParameters();

                    Vector2i r = Settings.Engine.Resolution;
                    UpdateRenderTargets(r.X, r.Y);

                    Loaded?.Invoke(this);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
            });
        }

        private void ExtractParameters()
        {
            RadiusParam = effect.Parameters["Radius"];
            FarClipParam = effect.Parameters["FarClip"];
            BiasParam = effect.Parameters["Bias"];
            GBufferPixelSizeParam = effect.Parameters["GBufferPixelSize"];
            RandomTileParam = effect.Parameters["RandomTile"];
            HalfBufferHalfPixelParam = effect.Parameters["HalfBufferHalfPixel"];
            TempBufferResParam = effect.Parameters["TempBufferRes"];
            SSAOResParam = effect.Parameters["SSAORes"];
            SSAOIntensityParam = effect.Parameters["SSAOIntensity"];
            BlurDirectionParam = effect.Parameters["BlurDirection"];
            SSAOBufferParam = effect.Parameters["SSAOBuffer"];
            GNormalBufferParam = effect.Parameters["GNormalBuffer"];
            GDepthBufferParam = effect.Parameters["GDepthBuffer"];
            RandomBufferParam = effect.Parameters["RandomBuffer"];

            renderTargetParam = effect.Parameters["RenderTarget"];

            //todo: make properties
            RadiusParam.SetValue(new Vector2(0.05f, 0.5f));
            FarClipParam.SetValue(engine.Camera.FarClip);
            BiasParam.SetValue(0.00001f);
            RandomTileParam.SetValue(100.0f);
            SSAOIntensityParam.SetValue(2.75f);

            RandomBufferParam.SetValue(GenerateRandomTexture(engine.GraphicsDevice));
        }

        public void UpdateRenderTargets(int w, int h)
        {
            renderTarget = new RenderTarget2D(engine.GraphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None);

            int half = 2;

            halfBuffer0 = new RenderTarget2D(engine.GraphicsDevice, w / half, h / half, false, SurfaceFormat.Rgba64, DepthFormat.None);
            halfBuffer1 = new RenderTarget2D(engine.GraphicsDevice, w / half, h / half, false, SurfaceFormat.Rgba64, DepthFormat.None); 

            GBufferPixelSizeParam.SetValue(new Vector2(0.5f / w, 0.5f / h));
            HalfBufferHalfPixelParam.SetValue(new Vector2(0.5f / w, 0.5f / h));
            TempBufferResParam.SetValue(new Vector2(w / half, h / half));
            SSAOResParam.SetValue(new Vector2(w / half, h / half));
        }

        public void Update(Camera camera, GameTime gameTime) { }

        public RenderTarget2D Draw(QuadRenderer quadRenderer, Texture2D previousPass)
        {
            engine.GraphicsDevice.SetRenderTarget(halfBuffer0);
            engine.GraphicsDevice.Clear(Color.Black.ToVector4());

            GNormalBufferParam.SetValue(engine.Scene.Graphics.GBuffer.Normal);
            GDepthBufferParam.SetValue(engine.Scene.Graphics.GBuffer.Depth);
            
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            engine.GraphicsDevice.BlendState = BlendState.Opaque;
            engine.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            effect.Programs["ssao"].Apply();
            quadRenderer.RenderQuad();

            //todo: make a property
            int blurCount = 1;
            if (blurCount > 0)
            {
                for (int i = 0; i < blurCount; i++)
                {
                    engine.GraphicsDevice.SetRenderTarget(halfBuffer1);
                    engine.GraphicsDevice.Clear(Color.Black.ToVector4());

                    SSAOBufferParam.SetValue(halfBuffer0);
                    BlurDirectionParam.SetValue(Vector2.UnitX / (float)halfBuffer1.Width);

                    effect.Programs["ssaoBlur"].Apply();
                    quadRenderer.RenderQuad();

                    engine.GraphicsDevice.SetRenderTarget(halfBuffer0);
                    engine.GraphicsDevice.Clear(Color.Black.ToVector4());

                    SSAOBufferParam.SetValue(halfBuffer1);
                    BlurDirectionParam.SetValue(Vector2.UnitY / (float)halfBuffer1.Height);

                    effect.Programs["ssaoBlur"].Apply();
                    quadRenderer.RenderQuad();
                }
            }

            engine.GraphicsDevice.SetRenderTarget(renderTarget);

            SSAOBufferParam.SetValue(halfBuffer0);
            renderTargetParam.SetValue(previousPass);

            engine.GraphicsDevice.BlendState = BlendState.Opaque;

            effect.Programs["ssaoFinal"].Apply();
            quadRenderer.RenderQuad();

            engine.GraphicsDevice.SetRenderTarget(null);
            return renderTarget;
        }
    }
}