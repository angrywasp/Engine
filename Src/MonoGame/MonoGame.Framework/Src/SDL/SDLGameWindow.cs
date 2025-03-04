// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework
{
    public class SdlGameWindow : IDisposable
    {
        public event EventHandler<TextInputEventArgs> TextInput;

        public event EventHandler<EventArgs> ClientSizeChanged;

        private IntPtr _handle;
        private bool _disposed;
        private bool _resizable, _borderless, _willBeFullScreen, _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName;
        private int _width, _height;
        private bool _wasMoved, _supressMoved;

        internal MouseState MouseState;

        internal bool IsTextInputHandled { get { return TextInput != null; } }

        public bool AllowUserResizing
        {
            get { return !IsBorderless && _resizable; }
            set
            {
                Sdl.Window.SetResizable(_handle, value);
                _resizable = value;
            }
        }

        public Rectangle ClientBounds
        {
            get
            {
                int x = 0, y = 0;
                Sdl.Window.GetPosition(Handle, out x, out y);
                return new Rectangle(x, y, _width, _height);
            }
        }

        public Point Position
        {
            get
            {
                int x = 0, y = 0;

                if (!IsFullScreen)
                    Sdl.Window.GetPosition(Handle, out x, out y);

                return new Point(x, y);
            }
            set
            {
                Sdl.Window.SetPosition(Handle, value.X, value.Y);
                _wasMoved = true;
            }
        }

        public IntPtr Handle => _handle;

        public bool IsBorderless
        {
            get { return _borderless; }
            set
            {
                Sdl.Window.SetBordered(_handle, value ? 0 : 1);
                _borderless = value;
            }
        }

        public static SdlGameWindow Instance;
        public uint? Id;
        public bool IsFullScreen;

        private readonly IGame game;

        public SdlGameWindow(IGame game)
        {
            this.game = game;
            _screenDeviceName = "";

            Instance = this;

            _width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _height = GraphicsDeviceManager.DefaultBackBufferHeight;

            Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            Sdl.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            _handle = Sdl.Window.Create("", 0, 0,
                GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight,
                Sdl.Window.State.Hidden);
        }

        internal void CreateWindow(string title)
        {
            var initflags =
                Sdl.Window.State.OpenGL |
                Sdl.Window.State.Hidden |
                Sdl.Window.State.InputFocus |
                Sdl.Window.State.MouseFocus;

            if (_handle != IntPtr.Zero)
                Sdl.Window.Destroy(_handle);

            var winx = Sdl.Window.PosCentered;
            var winy = Sdl.Window.PosCentered;

            // if we are on Linux, start on the current screen
            if (CurrentPlatform.OS == OS.Linux)
            {
                winx |= GetMouseDisplay();
                winy |= GetMouseDisplay();
            }

            _handle = Sdl.Window.Create(title,
                winx, winy, _width, _height, initflags);

            Id = Sdl.Window.GetWindowId(_handle);

            Sdl.Window.SetBordered(_handle, _borderless ? 0 : 1);
            Sdl.Window.SetResizable(_handle, _resizable);

            SetCursorVisible(_mouseVisible);
            Mouse.PrimaryWindow = this;
        }

        ~SdlGameWindow()
        {
            Dispose(false);
        }

        private static int GetMouseDisplay()
        {
            var rect = new Sdl.Rectangle();

            int x, y;
            Sdl.Mouse.GetGlobalState(out x, out y);

            var displayCount = Sdl.Display.GetNumVideoDisplays();
            for (var i = 0; i < displayCount; i++)
            {
                Sdl.Display.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width &&
                    y >= rect.Y && y < rect.Y + rect.Height)
                {
                    return i;
                }
            }

            return 0;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            Sdl.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _screenDeviceName = screenDeviceName;

            var prevBounds = ClientBounds;
            var displayIndex = Sdl.Window.GetDisplayIndex(Handle);

            Sdl.Rectangle displayRect;
            Sdl.Display.GetBounds(displayIndex, out displayRect);

            if (_willBeFullScreen != IsFullScreen || _hardwareSwitch != game.GraphicsDeviceManager.HardwareModeSwitch)
            {
                var fullscreenFlag = game.GraphicsDeviceManager.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;
                Sdl.Window.SetFullscreen(Handle, (_willBeFullScreen) ? fullscreenFlag : 0);
                _hardwareSwitch = game.GraphicsDeviceManager.HardwareModeSwitch;
            }

            if (!_willBeFullScreen || game.GraphicsDeviceManager.HardwareModeSwitch)
            {
                Sdl.Window.SetSize(Handle, clientWidth, clientHeight);
                _width = clientWidth;
                _height = clientHeight;
            }
            else
            {
                _width = displayRect.Width;
                _height = displayRect.Height;
            }

            int ignore, minx = 0, miny = 0;
            Sdl.Window.GetBorderSize(_handle, out miny, out minx, out ignore, out ignore);

            var centerX = Math.Max(prevBounds.X + ((prevBounds.Width - clientWidth) / 2), minx);
            var centerY = Math.Max(prevBounds.Y + ((prevBounds.Height - clientHeight) / 2), miny);

            if (IsFullScreen && !_willBeFullScreen)
            {
                // We need to get the display information again in case
                // the resolution of it was changed.
                Sdl.Display.GetBounds (displayIndex, out displayRect);

                // This centering only occurs when exiting fullscreen
                // so it should center the window on the current display.
                centerX = displayRect.X + displayRect.Width / 2 - clientWidth / 2;
                centerY = displayRect.Y + displayRect.Height / 2 - clientHeight / 2;
            }

            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            if ((Sdl.Patch > 4 || !AllowUserResizing) && !_wasMoved)
                Sdl.Window.SetPosition(Handle, centerX, centerY);

            if (IsFullScreen != _willBeFullScreen)
                ClientSizeChanged?.Invoke(this, EventArgs.Empty);

            IsFullScreen = _willBeFullScreen;

            _supressMoved = true;
        }

        internal void Moved()
        {
            if (_supressMoved)
            {
                _supressMoved = false;
                return;
            }

            _wasMoved = true;
        }

        public void ClientResize(int width, int height)
        {
            // SDL reports many resize events even if the Size didn't change.
            // Only call the code below if it actually changed.
            if (game.GraphicsDevice.PresentationParameters.BackBufferWidth == width &&
                game.GraphicsDevice.PresentationParameters.BackBufferHeight == height) {
                return;
            }
            game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
            game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;
            game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            Sdl.Window.GetSize(Handle, out _width, out _height);

            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CallTextInput(char c, Keys key = Keys.None)
        {
            TextInput?.Invoke(this, new TextInputEventArgs(c, key));
        }

        protected void SetTitle(string title)
        {
            Sdl.Window.SetTitle(_handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Sdl.Window.Destroy(_handle);
            _handle = IntPtr.Zero;

            _disposed = true;
        }
    }
}
