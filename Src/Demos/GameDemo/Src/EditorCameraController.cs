using System;
using System.Numerics;
using AngryWasp.Helpers;
using AngryWasp.Math;
using Engine;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Keyboard = Engine.Input.KeyboardExtensions;
using Mouse = Engine.Input.MouseExtensions;

namespace GameDemo
{
    public class EditorCameraController : ICameraController
    {
        private Degree yaw;
        private Degree pitch;
        private Vector3 position;

        private bool mlb = false;
        private Vector2 mlbPos = Vector2.Zero;

        private bool rotateAroundObject = false;

        public Degree Yaw { get; set; }

        public Degree Pitch { get; set; }

        public Vector3 Position { get; set; }

        public float SlowSpeed { get; set; } = 1;

        public float FastSpeed { get; set; } = 10;

        public float CalculatePitch(float distance, float height) => MathF.Atan2(distance, height);

        public void Update(GameTime gameTime)
        {
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Keyboard k = Engine.Input.Keyboard;
            Mouse m = Engine.Input.Mouse;

            float speed = k.ModifierKeyDown(Sdl.Keyboard.Keymod.LeftShift) ? FastSpeed : SlowSpeed;

            //use arrows to control yaw/pitch of camera
            /*if (k.KeyDown(Keys.Left))
                yaw += totalSeconds * 10 * speed;
            if (k.KeyDown(Keys.Right))
                yaw -= totalSeconds * 10 * speed;
            if (k.KeyDown(Keys.Up))
                pitch += totalSeconds * 10 * speed;
            if (k.KeyDown(Keys.Down))
                pitch -= totalSeconds * 10 * speed;*/

            var yawRad = yaw.ToRadians();

            //use WSAD to control position
            if (k.KeyDown(Keys.Q))
                position.Y -= speed * totalSeconds;
            if (k.KeyDown(Keys.E))
                position.Y += speed * totalSeconds;
            if (k.KeyDown(Keys.W))
            {
                position.Z -= speed * totalSeconds * MathF.Cos(yawRad);
                position.X -= speed * totalSeconds * MathF.Sin(yawRad);
            }
            if (k.KeyDown(Keys.S))
            {
                position.Z += speed * totalSeconds * MathF.Cos(yawRad);
                position.X += speed * totalSeconds * MathF.Sin(yawRad);
            }
            if (k.KeyDown(Keys.A))
            {
                position.Z += speed * totalSeconds * MathF.Sin(yawRad);
                position.X -= speed * totalSeconds * MathF.Cos(yawRad);
            }
            if (k.KeyDown(Keys.D))
            {
                position.Z -= speed * totalSeconds * MathF.Sin(yawRad);
                position.X += speed * totalSeconds * MathF.Cos(yawRad);
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
                    yaw -= 0.2f * delta.X;
                    pitch -= 0.2f * delta.Y;
                }
            }
            else
                mlb = false;

            Matrix4x4 pitchYaw = Matrix4x4.CreateRotationX(pitch.ToRadians()) * Matrix4x4.CreateRotationY(yawRad);
            if (rotateAroundObject)
                transform = Matrix4x4.CreateTranslation(position) * pitchYaw;
            else
                transform = pitchYaw * Matrix4x4.CreateTranslation(position);
        }

        #region ICameraController implementation

        private Matrix4x4 transform;

		public Matrix4x4 Transform
		{
			get { return transform; }
			set
			{
				transform = value;

				Vector3 scl;
				Quaternion quat;
				Matrix4x4.Decompose(value, out scl, out quat, out position);
				Vector3 rot = MathHelper.QuaternionToEuler(quat, true);
				yaw = rot.X;
				pitch = rot.Y;
			}
		}

		public EngineCore Engine { get; set; }

        #endregion
    }
}