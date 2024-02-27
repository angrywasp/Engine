using System.Threading.Tasks;
using AngryWasp.Logger;
using AngryWasp.Math;
using Engine.Cameras;
using Engine.World;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Objects.GameObjects.Controllers
{
    public class TurretControllerType : GameObjectControllerType
    {
    }

    public class TurretController : GameObjectController
    {
        #region Required in every script class

        private TurretControllerType _type = null;

        public new TurretControllerType Type => _type;

        #endregion

        private Turret controlledTurret;
        private Mode currentMode = Mode.Sweep;

        private enum Mode
        {
            Idle,
            Sweep,
            Target
        }

        public override void AssignControlledObject(GameObject gameObject)
        {
            base.AssignControlledObject(gameObject);

            controlledTurret = gameObject as Turret;

            if (controlledTurret == null)
            {
                Log.Instance.WriteFatal("TurretController can only be applied to a Turret");
                return;
            }
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            switch (currentMode)
            {
                case Mode.Sweep:
                    UpdateSweepMode(gameTime);
                    break;
                case Mode.Target:
                    UpdateTargetMode(gameTime);
                    break;
            }
        }

        bool turnRight = true;

        private void UpdateSweepMode(GameTime gameTime)
        {
            Radian x = controlledTurret.CalculateFrameTurnDistance(gameTime).ToRadians();

            //sweep back and forward
            //TurnRight/TurnLeft return false when the turn takes it to its Min/Max turn angle
            //so when it returns false, we switch directions
            if (turnRight)
                turnRight = controlledTurret.TurnRight(x);
            else
                turnRight = !controlledTurret.TurnLeft(x);
        }

        private void UpdateTargetMode(GameTime gameTime)
        {

        }
    }
}