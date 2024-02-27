using System.Threading.Tasks;
using Engine.Cameras;
using Engine.World;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class DynamicType : GameObjectType
    {
        [JsonProperty] public float LifeMin { get; set; } = 0;

        [JsonProperty] public float LifeMax { get; set; } = 100;
    }

    public class Dynamic : GameObject
    {
        #region Required in every script class

        private DynamicType _type = null;

        public new DynamicType Type => _type;

        #endregion

        private float life;
        private bool isDead = false;

        public float Life
        {
            get { return life; }
            set
            {
                if (life == value)
                    return;

                if (isDead)
                    return;

                life = value;

                if (life < _type.LifeMin)
                    life = _type.LifeMin;
                else if (life > _type.LifeMax)
                    life = _type.LifeMax;

                if (_type.LifeMax > _type.LifeMin) //dynamic can't die if min and max are set the same
                {
                    if (life <= _type.LifeMin)
                        Die();
                }
            }
        }

        public bool IsDead => isDead;

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine).ConfigureAwait(false);
            life = _type.LifeMax;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            foreach (var c in Components)
                if (c.Value.GlobalTransform.Translation.Y < -100)
                    UnloadComponent(c.Key);

            if (Components.Count == 0)
                Die();
        }

        protected virtual void Die()
        {
            if (isDead)
                return;

            engine.Scene.Map.QueueObjectRemove(this.UID);
            isDead = true;
        }
    }
}
