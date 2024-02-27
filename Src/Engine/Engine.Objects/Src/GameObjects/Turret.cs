using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Math;
using Engine.World.Components;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class TurretType : DynamicType
    {
        [JsonProperty] public float MinimumTurnAngle { get; set; } = -MathHelper.PiOver2;
        [JsonProperty] public float MaximumTurnAngle { get; set; } = MathHelper.PiOver2;
        [JsonProperty] public Degree TurnSpeed { get; set; } = 10f;//Turn speed in degrees/second
    }

    public class Turret : Dynamic
    {
        #region Required in every script class

        private TurretType _type = null;

        public new TurretType Type => _type;

        #endregion

        //private BEPUphysics.Entities.Entity turretBody;

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine).ConfigureAwait(false);

            MeshComponent baseComponent = (MeshComponent)Components["Base"];
            MeshComponent turretComponent = (MeshComponent)Components["Turret"];

            //turretBody = turretComponent.Body;

            /*CollisionGroup g = new CollisionGroup();
            baseComponent.Body.CollisionInformation.CollisionRules.Group = g;
            turretComponent.Body.CollisionInformation.CollisionRules.Group = g;
            CollisionGroup.DefineCollisionRule(g, g, CollisionRule.NoBroadPhase);*/
        }

        public Degree CalculateFrameTurnDistance(GameTime gameTime)
        {
            return _type.TurnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool TurnRight(Radian radians)
        {
            Radian r = GetDirection();
            r -= radians;

            if (r <= _type.MinimumTurnAngle)
            {
                TurnTo(_type.MinimumTurnAngle);
                return false;
            }

            TurnTo(r);
            return true;
        }

        public bool TurnLeft(Radian radians)
        {
            Radian r = GetDirection();
            r += radians;

            if (r >= _type.MaximumTurnAngle)
            {
                TurnTo(_type.MaximumTurnAngle);
                return false;
            }

            TurnTo(r);
            return true;
        }

        public void TurnTo(Radian f)
        {
            //turretBody.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, f);
        }

        public Radian GetDirection()
        {
            return 0;
            /*float q0 = turretBody.Orientation.W;
			float q1 = turretBody.Orientation.Y;
			float q2 = turretBody.Orientation.X;
			float q3 = turretBody.Orientation.Z;

			Radian x = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2)));
            return x;*/
        }
    }
}