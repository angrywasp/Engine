using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Graphics.Materials
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class MaterialBase
    {
        protected Effect effect;
        protected EngineCore engine;
        private bool isLoaded = false;

        public Effect Effect => effect;
        
        public bool IsLoaded => isLoaded;

        public async Task LoadAsync(EngineCore engine)
        {
            this.engine = engine;
            await new AsyncUiTask().Run(() => {
                effect = LoadEffect();
                ExtractParameters(effect);
                LoadResources(engine);
                this.isLoaded = true;
            }).ConfigureAwait(false);
        }

        protected abstract void LoadResources(EngineCore engine);

        public abstract void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection);

        public virtual void Apply(int pass) => effect.Programs[pass].Apply();

        public virtual void Apply(string name) => effect.Programs[name].Apply();

        public abstract Effect LoadEffect();

		public abstract void ExtractParameters(Effect effect);
    }
}
