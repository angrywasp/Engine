using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Editor.TerrainEditor.Brushes
{
    public abstract class TerrainBrush
    {
        private static readonly Vector4 TransparentBlack = Color.TransparentBlack.ToVector4();

        public delegate void RadiusChangedEventHandler(TerrainBrush sender, int radius);
        public event RadiusChangedEventHandler RadiusChanged;

        protected Texture2D mt;
        protected Effect e;
        protected EngineCore engine;
        private int rad;

        public Texture2D Mask => mt;

        public abstract string EffectFile { get; }

        public int Radius
        {
            get { return rad; }
            set
            {
                bool update = false;
                if (rad != value || Mask == null)
                    update = true;

                rad = value;

                if (update)
                {
                    GenerateMask();
                    if (RadiusChanged != null)
                        RadiusChanged(this, value);
                }
            }
        }

        public TerrainBrush(EngineCore engine, int radius)
        {
            this.engine = engine;
            this.rad = radius;
        }

        public async Task LoadAsync()
        {
            await new AsyncUiTask().Run(() => {
                LoadEffect();
                GenerateMask();
            }).ConfigureAwait(false);
        }

        private void LoadEffect()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/PostProcessing/PostProcess.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, EffectFile, null);

            e = new EffectBuilder().Start(engine.GraphicsDevice)
			.CreateProgram(vs, ps)
			.Finish();
        }

        private void GenerateMask()
        {
            if (rad == 0)
                Debugger.Break();

            int diameter = rad * 2;

            RenderTarget2D maskRt = new RenderTarget2D(engine.GraphicsDevice, diameter, diameter);
            engine.GraphicsDevice.SetRenderTarget(maskRt);
            engine.GraphicsDevice.Clear(TransparentBlack);
            e.DefaultProgram.Apply();
            engine.Scene.Graphics.Quad.RenderQuad();
            engine.GraphicsDevice.SetRenderTarget(null);

            mt = maskRt;
        }
    }
}