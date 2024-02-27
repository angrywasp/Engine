// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public class SdlGamePlatform
    {
        private readonly IGame game;
        private readonly List<Keys> keys;
        private int isExiting;
        private SdlGameWindow window;
        private bool disposed;
        private bool isMouseVisible;

        public bool IsActive { get; internal set; }

        public bool IsMouseVisible
        {
            get => isMouseVisible; 
            set
            {
                if (isMouseVisible != value)
                {
                    isMouseVisible = value;
                    window?.SetCursorVisible(value);
                }
            }
        }

        public SdlGameWindow Window => window;
        public bool IsDisposed => disposed;

        public SdlGamePlatform(IGame game)
        {
            this.game = game;
            this.keys = new List<Keys>();
            Keyboard.SetKeys(this.keys);

            Sdl.Version sversion;
            Sdl.GetVersion(out sversion);

            Sdl.Major = sversion.Major;
            Sdl.Minor = sversion.Minor;
            Sdl.Patch = sversion.Patch;

            Sdl.Init((int)(
                Sdl.InitFlags.Video |
                Sdl.InitFlags.Joystick |
                Sdl.InitFlags.GameController |
                Sdl.InitFlags.Haptic
            ));

            Sdl.DisableScreenSaver();

            GamePad.InitDatabase();
            
            window = new SdlGameWindow(game);
        }

        ~SdlGamePlatform()
        {
            Dispose(false);
        }

        public void BeforeInitialize ()
        {
            SdlRunLoop();
            IsActive = true;
        }

        internal void OnPresentationChanged(object sender, PresentationParameters pp)
        {
            var displayIndex = Sdl.Window.GetDisplayIndex(Window.Handle);
            var displayName = Sdl.Display.GetDisplayName(displayIndex);
            BeginScreenDeviceChange(pp.IsFullScreen);
            EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight);
        }

        public void RunLoop()
        {
            if (window != null)
                Sdl.Window.Show(Window.Handle);

            while (true)
            {
                SdlRunLoop();
                game.Tick();
                Threading.Run();
                GraphicsDevice.DisposeContexts();

                if (isExiting > 0)
                    break;
            }
        }

        private void SdlRunLoop()
        {
            Sdl.Event ev;

            while (Sdl.PollEvent(out ev) == 1)
            {
                if (ev.Type == Sdl.EventType.Quit)
                    isExiting++;
                else if (ev.Type == Sdl.EventType.JoyDeviceAdded)
                    Joystick.AddDevice(ev.JoystickDevice.Which);
                else if (ev.Type == Sdl.EventType.ControllerDeviceRemoved)
                    GamePad.RemoveDevice(ev.ControllerDevice.Which);
                else if (ev.Type == Sdl.EventType.JoyDeviceRemoved)
                    Joystick.RemoveDevice(ev.JoystickDevice.Which);
                else if (ev.Type == Sdl.EventType.MouseWheel)
                {
                    const int wheelDelta = 120;
                    Mouse.ScrollY += ev.Wheel.Y * wheelDelta;
                    Mouse.ScrollX += ev.Wheel.X * wheelDelta;
                }
                else if (ev.Type == Sdl.EventType.MouseMotion)
                {
                    Window.MouseState.X = ev.Motion.X;
                    Window.MouseState.Y = ev.Motion.Y;
                }
                else if (ev.Type == Sdl.EventType.KeyDown)
                {
                    var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                    if (!keys.Contains(key))
                        keys.Add(key);
                    char character = (char)ev.Key.Keysym.Sym;
                    if (char.IsControl(character))
                        window.CallTextInput(character, key);
                }
                else if (ev.Type == Sdl.EventType.KeyUp)
                {
                    var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                    keys.Remove(key);
                }
                else if (ev.Type == Sdl.EventType.TextInput)
                {
                    if (window.IsTextInputHandled)
                    {
                        int len = 0;
                        int utf8character = 0; // using an int to encode multibyte characters longer than 2 bytes
                        byte currentByte = 0;
                        int charByteSize = 0; // UTF8 char lenght to decode
                        unsafe
                        {
                            while ((currentByte = Marshal.ReadByte((IntPtr)ev.Text.Text, len)) != 0)
                            {
                                // we're reading the first UTF8 byte, we need to check if it's multibyte
                                if (charByteSize == 0)
                                {
                                    if (currentByte < 192)
                                        charByteSize = 1;
                                    else if (currentByte < 224)
                                        charByteSize = 2;
                                    else if (currentByte < 240)
                                        charByteSize = 3;
                                    else
                                        charByteSize = 4;

                                    utf8character = 0;
                                }

                                // assembling the character
                                utf8character <<= 8;
                                utf8character |= currentByte;

                                charByteSize--;
                                if (charByteSize == 0) // finished decoding the current character
                                {
                                    var key = KeyboardUtil.ToXna(utf8character);
                                    window.CallTextInput((char)utf8character, key);
                                }

                                len++;
                            }
                        }
                    }
                }
                else if (ev.Type == Sdl.EventType.WindowEvent)
                {
                    if (ev.Window.WindowID == window.Id)
                    {
                        if (ev.Window.EventID == Sdl.Window.EventId.Resized || ev.Window.EventID == Sdl.Window.EventId.SizeChanged)
                            window.ClientResize(ev.Window.Data1, ev.Window.Data2);
                        else if (ev.Window.EventID == Sdl.Window.EventId.FocusGained)
                            IsActive = true;
                        else if (ev.Window.EventID == Sdl.Window.EventId.FocusLost)
                            IsActive = false;
                        else if (ev.Window.EventID == Sdl.Window.EventId.Moved)
                            window.Moved();
                        else if (ev.Window.EventID == Sdl.Window.EventId.Close)
                            isExiting++;
                    }
                }
            }
        }

        public void Exit()
        {
            Interlocked.Increment(ref isExiting);
        }

        public void BeginScreenDeviceChange(bool willBeFullScreen) =>
            window.BeginScreenDeviceChange(willBeFullScreen);

        public void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight) => window.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);

        public void Present()
        {
            if (game.GraphicsDevice != null)
                game.GraphicsDevice.Present();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (window != null)
            {
                window.Dispose();
                window = null;

                Joystick.CloseDevices();
                Sdl.Quit();
            }

            if (!disposed)
            {
                Mouse.PrimaryWindow = null;
                disposed = true;
            }
        }
    }
}
