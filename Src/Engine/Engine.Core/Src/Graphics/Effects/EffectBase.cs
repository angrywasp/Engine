using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public abstract class EffectBase : IDisposable
    {
        protected Effect effect;
        protected GraphicsDevice graphicsDevice;

        public Effect Effect => effect;

        public async Task LoadAsync(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            await new AsyncUiTask().Run(() => {
                effect = LoadEffect();
                ExtractParameters(effect);
            }).ConfigureAwait(false);
        }

        ~EffectBase()
        {
            Dispose();
        }

        protected abstract Effect LoadEffect();

        protected abstract void ExtractParameters(Effect effect);

        bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                effect.Dispose();
                disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }

    public abstract class WorldEffect : EffectBase
    {
        protected EffectParameter worldParam;
		protected EffectParameter viewParam;
		protected EffectParameter projectionParam;

        public Matrix4x4 World { set => worldParam.SetValue(value); }

		public Matrix4x4 View { set => viewParam.SetValue(value); }

		public Matrix4x4 Projection { set => projectionParam.SetValue(value); }

        protected override void ExtractParameters(Effect effect)
		{
			worldParam = effect.Parameters["World"];
			viewParam = effect.Parameters["View"];
			projectionParam = effect.Parameters["Projection"];
		}
    }
}
