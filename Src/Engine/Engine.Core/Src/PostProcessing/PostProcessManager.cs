using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Engine.Configuration;
using System.Linq;
using Engine.Cameras;

namespace Engine.PostProcessing
{
    public class PostProcessManager
    {
        private QuadRenderer quadRenderer;
        private GraphicsDevice graphicsDevice;
        private EngineCore engine;

        private Texture2D renderTarget;

        public Texture2D RenderTarget => renderTarget;

        private List<IPostProcess> postProcesses = new List<IPostProcess>();

        public IPostProcess this[string value] => postProcesses.Where(x => x.Name == value).FirstOrDefault();

        public PostProcessManager(EngineCore engine)
        {
            this.engine = engine;
            graphicsDevice = engine.GraphicsDevice;
            quadRenderer = engine.Scene.Graphics.Quad;

            Resize(Settings.Engine.Resolution.X, Settings.Engine.Resolution.Y);
        }

        public void Resize(int w, int h)
        {
            renderTarget = new RenderTarget2D(graphicsDevice, w, h,
                false, SurfaceFormat.Rgba64, DepthFormat.Depth32F,
                0, RenderTargetUsage.DiscardContents);

            renderTarget.Name = "PostProcessorBuffer";

            foreach (IPostProcess pp in postProcesses)
                pp.UpdateRenderTargets(w, h);
        }

        public void Add(string key, IPostProcess value)
        {
            if (this[key] != null)
                return;

            value.Loaded += (p) =>
            {
                if (this[key] != null)
                    return;
                
                value.Name = key;
                postProcesses.Add(p);
            };

            value.Init(engine);
        }

        public void Insert(int index, string key, IPostProcess value)
        {
            if (this[key] != null)
                return;

            value.Loaded += (p) =>
            {
                if (this[key] != null)
                    return;
                
                value.Name = key;
                if (index >= postProcesses.Count)
                    postProcesses.Add(p);
                else
                    postProcesses.Insert(index, p);
            };

            value.Init(engine);
        }

        public void Remove(string key)
        {
            var pp = this[key];
            if (pp == null)
                return;

            postProcesses.Remove(pp);
        }

        public void Clear() => postProcesses.Clear();

        public void Update(Camera camera, GameTime gameTime)
        {
            foreach (IPostProcess pp in postProcesses)
                pp.Update(camera, gameTime);
        }

        public void Draw(Texture2D source)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            if (postProcesses.Count == 0)
            {
                graphicsDevice.SetRenderTarget(null);
                renderTarget = source;
                return;
            }

            RenderTarget2D rt = null;
            foreach (IPostProcess pp in postProcesses)
            {
                rt = pp.Draw(quadRenderer, source);
                source = rt;
            }

            renderTarget = rt;
        }
    }
}