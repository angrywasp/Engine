using System;
using System.Numerics;
using AngryWasp.Helpers;
using AngryWasp.Math;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = Engine.Input.KeyboardExtensions;
using Mouse = Engine.Input.MouseExtensions;

namespace Engine.Editor.Components
{
    public class EditorCameraController : ICameraController
    {
        private bool mlb = false;
        private Vector2 mlbPos = Vector2.Zero;

        public bool RotateAroundObject { get; set; } = false;

        public bool UpdatePosition { get; set; } = true;

        public Degree Yaw { get; set; }

        public Degree Pitch { get; set; }

        public Vector3 Position { get; set; }

        public float SlowSpeed { get; set; } = 1;

        public float FastSpeed { get; set; } = 10;

        public void Update(GameTime gameTime)
        {
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Keyboard k = Engine.Input.Keyboard;
            Mouse m = Engine.Input.Mouse;

            float speed = k.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ? FastSpeed : SlowSpeed;

            var yawRad = Yaw.ToRadians();
            var pos = Position;

            if (UpdatePosition)
            {
                //use WSAD to control position
                if (k.KeyDown(Keys.Q))
                    pos.Y -= speed * totalSeconds;
                if (k.KeyDown(Keys.E))
                    pos.Y += speed * totalSeconds;
                if (k.KeyDown(Keys.W))
                {
                    pos.Z -= speed * totalSeconds * MathF.Cos(yawRad);
                    pos.X -= speed * totalSeconds * MathF.Sin(yawRad);
                }
                if (k.KeyDown(Keys.S))
                {
                    pos.Z += speed * totalSeconds * MathF.Cos(yawRad);
                    pos.X += speed * totalSeconds * MathF.Sin(yawRad);
                }
                if (k.KeyDown(Keys.A))
                {
                    pos.Z += speed * totalSeconds * MathF.Sin(yawRad);
                    pos.X -= speed * totalSeconds * MathF.Cos(yawRad);
                }
                if (k.KeyDown(Keys.D))
                {
                    pos.Z -= speed * totalSeconds * MathF.Sin(yawRad);
                    pos.X += speed * totalSeconds * MathF.Cos(yawRad);
                }

                Position = pos;
            }

            //todo: use the Dragging info from the mouse class instead of this
            //use mouse to rotate camera
            if (m.ButtonDown(m.LeftButton))
            {
                if (!mlb)
                {
                    mlb = true;
                    mlbPos = m.Position.ToVector2();
                }
                else
                {
                    Vector2 mPos = m.Position.ToVector2();
                    Vector2 delta = mPos - mlbPos;
                    mlbPos = mPos;
                    Yaw -= 0.2f * delta.X;
                    Pitch -= 0.2f * delta.Y;
                }
            }
            else
                mlb = false;

            Matrix4x4 pitchYaw = Matrix4x4.CreateRotationX(Pitch.ToRadians()) * Matrix4x4.CreateRotationY(yawRad);
            if (RotateAroundObject)
                transform = Matrix4x4.CreateTranslation(Position) * pitchYaw;
            else
                transform = pitchYaw * Matrix4x4.CreateTranslation(Position);
        }

        #region ICameraController implementation

        private Matrix4x4 transform;

		public Matrix4x4 Transform
		{
			get { return transform; }
			set
			{
				transform = value;

				Matrix4x4.Decompose(value, out Vector3 scl, out Quaternion quat, out Vector3 pos);
				Vector3 rot = MathHelper.QuaternionToEuler(quat, true);
				Yaw = rot.X;
				Pitch = rot.Y;
                Position = pos;
			}
		}

		public EngineCore Engine { get; set; }

        #endregion
    }
}