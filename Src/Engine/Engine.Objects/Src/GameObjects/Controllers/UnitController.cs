using Microsoft.Xna.Framework;
using Engine.Cameras;
using Engine.World.Objects;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Engine.Objects.GameObjects.Pickups;
using System.Diagnostics;

namespace Engine.Objects.GameObjects.Controllers
{
    public class UnitControllerType : GameObjectControllerType
    {
        [JsonProperty] public float TakeItemsRadius { get; set; } = 2.0f;
        [JsonProperty] public bool AllowTakeItems { get; set; } = false;
    }

    public class UnitController : GameObjectController
    {
        private UnitControllerType _type = null;

        public new UnitControllerType Type => _type;

        private float takeItemsTimer;
        
        public float TakeItemsRadius { get; set; }
        public bool AllowTakeItems { get; set; }

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine).ConfigureAwait(false);
            takeItemsTimer = engine.Random.NextFloat();
            this.TakeItemsRadius = _type.TakeItemsRadius;
            this.AllowTakeItems = _type.AllowTakeItems;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            if (AllowTakeItems)
                UpdateTakeItems(camera, gameTime);
        }

        private void UpdateTakeItems(Camera camera, GameTime gameTime)
        {
            //we don't want or need this to run on every update.
            //so add a delay between updates
            takeItemsTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (takeItemsTimer > 0)
                return;

            takeItemsTimer += 0.5f;

            engine.Scene.Map.GetObjects(
                new BoundingSphere(Transform.Translation, TakeItemsRadius),
                (obj) =>
                {
                    if (obj.GameObject is Pickup pickup && !pickup.IsDead)
                        pickup.Take((Unit)this.ControlledObject);
                }
            );
        }
    }
}
