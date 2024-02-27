using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;

namespace Engine.Input
{
    public class GamePadExtensions
    {
        public const short LeftThumbDeadZone = 7849;
        public const short RightThumbDeadZone = 8689;

        private GamePadState state;
        private GamePadState stateLastFrame;

        private PlayerIndex index;

        public PlayerIndex Index => index;

        public void Init(PlayerIndex index)
        {
            this.index = index;
            //GamePadCapabilities caps = Microsoft.Xna.Framework.Input.GamePad.GetCapabilities(index);
        }

        public ButtonState A => state.Buttons.A;

        public ButtonState B => state.Buttons.B;

        public ButtonState X => state.Buttons.X;

        public ButtonState Y => state.Buttons.Y;

        public ButtonState Start => state.Buttons.Start;

        public ButtonState Back => state.Buttons.Back;

        public ButtonState LeftShoulder => state.Buttons.LeftShoulder;

        public ButtonState RightShoulder => state.Buttons.RightShoulder;

        public ButtonState LeftThumbButton => state.Buttons.LeftStick;

        public ButtonState RightThumbButton => state.Buttons.RightStick;

        public ButtonState BigButton => state.Buttons.BigButton;

        public ButtonState DPadDown => state.DPad.Down;

        public ButtonState DPadUp => state.DPad.Up;

        public ButtonState DPadLeft => state.DPad.Left;

        public ButtonState DPadRight => state.DPad.Right;

        public Vector2 LeftThumbStick => state.ThumbSticks.Left;

        public Vector2 RightThumbStick => state.ThumbSticks.Right;

        public Vector2 LeftThumbStickDelta => 
            new Vector2(state.ThumbSticks.Left.X - stateLastFrame.ThumbSticks.Left.X, state.ThumbSticks.Left.Y - stateLastFrame.ThumbSticks.Left.Y);

        public Vector2 RightThumbStickDelta =>
            new Vector2(state.ThumbSticks.Right.X - stateLastFrame.ThumbSticks.Right.X, state.ThumbSticks.Right.Y - stateLastFrame.ThumbSticks.Right.Y);

        public float LeftTrigger => state.Triggers.Left;

        public float RightTrigger => state.Triggers.Right;

        public float LeftTriggerDelta => state.Triggers.Left - stateLastFrame.Triggers.Left;

        public float RightTriggerDelta => state.Triggers.Right - stateLastFrame.Triggers.Right;

        public bool ButtonJustPressed(Buttons btn) => state.IsButtonDown(btn) && stateLastFrame.IsButtonUp(btn);

        public bool ButtonJustReleased(Buttons btn) => state.IsButtonUp(btn) && stateLastFrame.IsButtonDown(btn);

        public bool ButtonDown(Buttons btn) => state.IsButtonDown(btn);

        public bool ButtonUp(Buttons btn) => state.IsButtonUp(btn);

        public bool StateChanged => state.PacketNumber != stateLastFrame.PacketNumber;

        public bool IsConnected => state.IsConnected;

        public void Update(GameTime gameTime)
        {
            stateLastFrame = state;
            state = Microsoft.Xna.Framework.Input.GamePad.GetState(index, GamePadDeadZone.Circular);
        }
    }
}