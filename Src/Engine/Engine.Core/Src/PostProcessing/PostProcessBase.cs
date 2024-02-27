using Engine.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Configuration;
using Engine.Cameras;
using System;
using AngryWasp.Logger;
using Engine.Graphics.Effects.Builder;

namespace Engine.PostProcessing
{
    public interface IPostProcess
    {
        event EngineEventHandler<IPostProcess> Loaded;

        string Name { get; set; }

        void Init(EngineCore engine);

        void UpdateRenderTargets(int w, int h);

        void Update(Camera camera, GameTime gameTime);

        RenderTarget2D Draw(QuadRenderer quadRenderer, Texture2D previousPass);
    }

    public abstract class PostProcessBase : IPostProcess
    {
        public event EngineEventHandler<IPostProcess> Loaded;

        protected Effect effect;
		protected EngineCore engine;

        //The result of the render. Every post process needs one
        private EffectParameter renderTargetParam;
        private RenderTarget2D renderTarget;

        public abstract string EffectFile { get; }

        public string Name { get; set; }

        public virtual string[] IncludeDirs { get; } = null;

        public void Init(EngineCore engine)
        {
            this.engine = engine;

            Threading.BlockOnUIThread(() =>
            {
                try
                {
                    var vs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/PostProcessing/PostProcess.vert.glsl");
                    var ps = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, EffectFile, IncludeDirs);

                    effect = new EffectBuilder().Start(engine.GraphicsDevice)
                    .CreateProgram(vs, ps)
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

        protected virtual void ExtractParameters()
        {
            renderTargetParam = effect.Parameters["RenderTarget"];
        }

        public virtual void Update(Camera camera, GameTime gameTime) { }

        public virtual RenderTarget2D Draw(QuadRenderer quadRenderer, Texture2D previousPass)
        {
            engine.GraphicsDevice.SetRenderTarget(renderTarget);

            renderTargetParam.SetValue(previousPass);
            effect.DefaultProgram.Apply();
            quadRenderer.RenderQuad();
            engine.GraphicsDevice.SetRenderTarget(null);
            return renderTarget;
        }

        public virtual void UpdateRenderTargets(int w, int h) =>
            renderTarget = new RenderTarget2D(engine.GraphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.None);
    }
}