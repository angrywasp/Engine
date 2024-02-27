using AngryWasp.Math;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Cameras;
using Engine.World.Objects;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects.Controllers
{
    public class PickupControllerType : GameObjectControllerType
    {
        [JsonProperty] public Degree SpinSpeed { get; set; }
    }

    public class PickupController : GameObjectController
    {
        private PickupControllerType _type = null;

        public new PickupControllerType Type => _type;

        private Degree spinSpeed;
        private float rotation;

        public override async Task LoadAsync(EngineCore engine)
        {
            spinSpeed = _type.SpinSpeed;
            await base.LoadAsync(engine);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            rotation += (spinSpeed / FPSCounter.FramePerSecond).ToRadians();
            ControlledObject?.Transform.Update(Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotation), ControlledObject.Transform.Translation);
        }
    }
}