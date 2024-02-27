using Engine.Cameras;
using Microsoft.Xna.Framework;
using Engine.Objects.GameObjects;
using System.Numerics;
using Engine.World;

namespace Engine.Objects.Controllers.CameraControllers
{
    //only for use with types of unit or above
    //which would be pretty much everything one would want to base a camera controller on
    public class PlayerFps : Base
    {
        Unit unit;

        public PlayerFps(Camera c, GameObject g) : base(c, g)
        {
            unit = (Unit)g;
            
        }

        public override void Update(GameTime gameTime)
        {
            Matrix4x4 qm = Matrix4x4.CreateFromQuaternion(unit.Transform.Rotation);
            Transform = Matrix4x4.CreateRotationX(Pitch) * Matrix4x4.CreateWorld(unit.Transform.Translation + unit.Type.FpsCameraOffset, qm.Forward(), qm.Up());
        }
    }
}
