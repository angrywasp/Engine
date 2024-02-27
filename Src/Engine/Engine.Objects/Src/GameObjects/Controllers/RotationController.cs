using System.Numerics;
using System.Threading.Tasks;
using Engine.Cameras;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Objects.GameObjects.Controllers
{
    public class RotationControllerType : GameObjectControllerType
    {
    }

    public class RotationController : GameObjectController
    {
        private RotationControllerType _type = null;

        public new RotationControllerType Type => _type;

        float x, y, z;
        int xDir, yDir, zDir;

        float xInc, yInc, zInc;

        public override async Task LoadAsync(EngineCore engine)
        {
            x = engine.Random.Next(0, 360);
            y = engine.Random.Next(0, 360);
            z = engine.Random.Next(0, 360);

            xDir = x > 180 ? 1 : -1;
            yDir = y > 180 ? 1 : -1;
            zDir = z > 180 ? 1 : -1;

            xInc = engine.Random.NextFloat(0.0001f, 0.01f);
            yInc = engine.Random.NextFloat(0.0001f, 0.01f);
            zInc = engine.Random.NextFloat(0.0001f, 0.01f);

            await base.LoadAsync(engine);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            x += (xInc * xDir);
            y += (yInc * yDir);
            z += (zInc * zDir);

            Quaternion q = Quaternion.CreateFromYawPitchRoll(x, y, z);

            ControlledObject?.Transform.Update(q, ControlledObject.Transform.Translation);
        }
    }
}