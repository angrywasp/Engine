// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static class Mouse
    {
        internal static SdlGameWindow PrimaryWindow;

        private static readonly MouseState _defaultState = new MouseState();

        public static IntPtr WindowHandle => PrimaryWindow.Handle;

        internal static int ScrollX;
        internal static int ScrollY;

        public static MouseState GetState()
        {
            if (PrimaryWindow != null)
            {
                int x, y;
                var winFlags = Sdl.Window.GetWindowFlags(PrimaryWindow.Handle);
                var state = Sdl.Mouse.GetGlobalState(out x, out y);

                if ((winFlags & Sdl.Window.State.MouseFocus) != 0)
                {
                    // Window has mouse focus, position will be set from the motion event
                    PrimaryWindow.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
                    PrimaryWindow.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
                    PrimaryWindow.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
                    PrimaryWindow.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
                    PrimaryWindow.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

                    PrimaryWindow.MouseState.HorizontalScrollWheelValue = ScrollX;
                    PrimaryWindow.MouseState.ScrollWheelValue = ScrollY;
                }
                else
                {
                    // Window does not have mouse focus, we need to manually get the position
                    var clientBounds = PrimaryWindow.ClientBounds;
                    PrimaryWindow.MouseState.X = x - clientBounds.X;
                    PrimaryWindow.MouseState.Y = y - clientBounds.Y;
                }

                return PrimaryWindow.MouseState;
            }

            return _defaultState;
        }

        public static void SetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        public static void SetCursor(MouseCursor cursor)
        {
            Sdl.Mouse.SetCursor(cursor.Handle);
        }
    }
}
