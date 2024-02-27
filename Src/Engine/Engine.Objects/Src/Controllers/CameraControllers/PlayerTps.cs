using Microsoft.Xna.Framework;
using Engine.Cameras;
using Engine.Objects.GameObjects;
using System.Numerics;
using Engine.World;

namespace Engine.Objects.Controllers.CameraControllers
{
    //only for use with types of unit or above
    //which would be pretty much everything one would want to base a camera controller on
    public class PlayerTps : Base
    {
        Unit unit;

        public PlayerTps(Camera c, GameObject g) : base(c, g)
        {
            unit = (Unit)g;
        }

        public override void Update(GameTime gameTime)
        {
            Matrix4x4 qm = Matrix4x4.CreateRotationX(Pitch) * Matrix4x4.CreateFromQuaternion(unit.Transform.Rotation);
            Vector3 characterPosAdjusted = unit.Transform.Translation + (Vector3.UnitY * ((CharacterType)unit.Type).Height);

            Vector3 pos = ((UnitType)unit.Type).TpsCameraOffset;
            pos = Vector3.Transform(pos, qm);
            pos += characterPosAdjusted;

            Vector3 up = Vector3.UnitY;
            up = Vector3.Transform(up, qm);

            Matrix4x4 t;
            Matrix4x4.Invert(Matrix4x4.CreateLookAt(pos, characterPosAdjusted, up), out t);
            Transform = t;
        }
    }
}
