using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input
{
	[Flags]
    public enum MouseDirection
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }

    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
        X1 = 8,
        X2 = 16
    }

    public enum MouseScrollDirection
    {
        None,
        Up,
        Down
    }

    public class MouseExtensions
    {
        private MouseState state;
        private MouseState stateLastFrame;
        private Vector2 startDraggingPos;
        private GraphicsDevice graphicsDevice;

        public MouseState State => state;
        public MouseState StateLastFrame => stateLastFrame;
        

        public Vector2i Position
        {
            get { return new Vector2i(state.X, state.Y); }
            set { Microsoft.Xna.Framework.Input.Mouse.SetPosition(value.X, value.Y); }
        }

        public ButtonState RightButton => state.RightButton;

        public ButtonState MiddleButton => state.MiddleButton;

        public ButtonState LeftButton => state.LeftButton;

        public ButtonState XButton1 => state.XButton1;

        public ButtonState XButton2 => state.XButton2;

        public bool ButtonDown(ButtonState btn) => btn == ButtonState.Pressed;

        public bool ButtonUp(ButtonState btn) => btn == ButtonState.Released;

        public bool RightJustPressed => state.RightButton == ButtonState.Pressed && stateLastFrame.RightButton == ButtonState.Released;

        public bool RightJustReleased => state.RightButton == ButtonState.Released && stateLastFrame.RightButton == ButtonState.Pressed;

        public bool LeftJustPressed => state.LeftButton == ButtonState.Pressed && stateLastFrame.LeftButton == ButtonState.Released;

        public bool LeftJustReleased => state.LeftButton == ButtonState.Released && stateLastFrame.LeftButton == ButtonState.Pressed;

        public bool MiddleJustPressed => state.MiddleButton == ButtonState.Pressed && stateLastFrame.MiddleButton == ButtonState.Released; 

        public bool MiddleJustReleased => state.MiddleButton == ButtonState.Released && stateLastFrame.MiddleButton == ButtonState.Pressed;

        public bool X1JustPressed => state.XButton1 == ButtonState.Pressed && stateLastFrame.XButton1 == ButtonState.Released;

        public bool X1JustReleased => state.XButton1 == ButtonState.Released && stateLastFrame.XButton1 == ButtonState.Pressed;

        public bool X2JustPressed => state.XButton2 == ButtonState.Pressed && stateLastFrame.XButton2 == ButtonState.Released;

        public bool X2JustReleased => state.XButton2 == ButtonState.Released && stateLastFrame.XButton2 == ButtonState.Pressed;

        public int ScrollDelta
        {
            get
            {
                int delta = state.ScrollWheelValue - stateLastFrame.ScrollWheelValue;
                int returnValue = delta / 120;
                return returnValue; 
            }
        }

        public Vector2 MovementDelta => new Vector2(state.X - stateLastFrame.X, state.Y - stateLastFrame.Y);

        public Vector2 DragAmount => startDraggingPos - new Vector2(state.X, state.Y);

        public void ResetDragAmount() => startDraggingPos = new Vector2(state.X, state.Y);

        public bool InsideRect(Rectangle rect)
        {
            bool ret = state.X >= rect.X &&
                state.Y >= rect.Y &&
                state.X < rect.Right &&
                state.Y < rect.Bottom;
            return ret;
        }

        public MouseExtensions(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Update(GameTime gameTime)
        {
            stateLastFrame = state;
            state = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (state.LeftButton == ButtonState.Released)
                startDraggingPos = new Vector2(state.X, state.Y);
        }

        public Ray ToRay(Matrix4x4 _view, Matrix4x4 _projection)
        {
            Vector3 nearPoint = new Vector3(state.X, state.Y, 0);
            Vector3 farPoint = new Vector3(state.X, state.Y, 1);

            nearPoint = graphicsDevice.Viewport.Unproject(nearPoint,
                                                     _projection,
                                                     _view,
                                                     Matrix4x4.Identity);
            farPoint = graphicsDevice.Viewport.Unproject(farPoint,
                                                    _projection,
                                                    _view,
                                                    Matrix4x4.Identity);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

            return new Ray(nearPoint, direction, 1000);
        }

        public void CalculateMouseDirection(Vector2 delta, out MouseDirection dir)
        {
            dir = MouseDirection.None;

            if (delta.Y < 0)
                dir = MouseDirection.Up;
            else if (delta.Y > 0)
                dir = MouseDirection.Down;

            if (delta.X > 0)
                dir |= MouseDirection.Right;
            else if (delta.X < 0)
                dir |= MouseDirection.Left;
        }

        public void CalculateMouseScrollDirectionAndDelta(out MouseScrollDirection dir, out int delta)
        {
            dir = MouseScrollDirection.None;
            delta = ScrollDelta;

            if (delta < 0)
                dir = MouseScrollDirection.Down;
            else if (delta > 0)
                dir = MouseScrollDirection.Up;
        }

        public MouseButtons GetPressedButtons()
        {
            int x, y;
            var ms = Sdl.Mouse.GetState(out x, out y);

            return (MouseButtons)ms;

            /*MouseButtons mb = MouseButtons.None;

            mb |= (ms & Sdl.Mouse.Button.Left) != 0 ? MouseButtons.Left : MouseButtons.None;
            mb |= (ms & Sdl.Mouse.Button.Right) != 0 ? MouseButtons.Right : MouseButtons.None;
            mb |= (ms & Sdl.Mouse.Button.Middle) != 0 ? MouseButtons.Middle : MouseButtons.None;
            mb |= (ms & Sdl.Mouse.Button.X1Mask) != 0 ? MouseButtons.X1 : MouseButtons.None;
            mb |= (ms & Sdl.Mouse.Button.X2Mask) != 0 ? MouseButtons.X2 : MouseButtons.None;
            return mb;*/
        }
    }
}
