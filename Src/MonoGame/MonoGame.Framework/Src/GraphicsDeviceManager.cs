// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager : IDisposable
    {
        private GraphicsDevice graphicsDevice;
        private IGame game;

        public GraphicsDevice GraphicsDevice => graphicsDevice;

        private bool _initialized = false;

        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;
        private bool _adaptiveSync;
        private bool _verticalSync;
        private bool _disposed;
        private bool _hardwareModeSwitch = false;
        private bool _wantFullScreen;
        // dirty flag for ApplyChanges
        private bool _shouldApplyChanges;

        public static readonly int DefaultBackBufferWidth = 1280;
        public static readonly int DefaultBackBufferHeight = 720;

        public IntPtr WindowHandle { get; set; } = IntPtr.Zero;

        public GraphicsDeviceManager(IGame game)
        {
            if (game == null)
                throw new ArgumentNullException("game", "Game cannot be null.");

            this.game = game;

            _preferredBackBufferWidth = DefaultBackBufferWidth;
            _preferredBackBufferHeight = DefaultBackBufferHeight;

            _wantFullScreen = false;
        }

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void CreateDevice()
        {
            if (graphicsDevice != null)
                return;

            if (!_initialized)
                Initialize(game.WindowTitle);

            CreateDevice(DoPreparingDeviceSettings());
        }

        private void CreateDevice(GraphicsDeviceInformation gdi)
        {
            if (graphicsDevice != null)
                return;

            graphicsDevice = new GraphicsDevice(gdi);
            _shouldApplyChanges = false;

            // update the touchpanel display size when the graphicsdevice is reset
            graphicsDevice.PresentationChanged += game.Platform.OnPresentationChanged;
        }

        private GraphicsDeviceInformation DoPreparingDeviceSettings()
        {
            var gdi = new GraphicsDeviceInformation();
            PrepareGraphicsDeviceInformation(gdi);
            return gdi;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (graphicsDevice != null)
                    {
                        graphicsDevice.Dispose();
                        graphicsDevice = null;
                    }
                }
                _disposed = true;
            }
        }

        private void PreparePresentationParameters(PresentationParameters presentationParameters)
        {
            presentationParameters.BackBufferFormat = SurfaceFormat.Rgba;
            presentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            presentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;
            presentationParameters.IsFullScreen = _wantFullScreen;
            presentationParameters.HardwareModeSwitch = _hardwareModeSwitch;
            presentationParameters.PresentationInterval = (_adaptiveSync && graphicsDevice.GraphicsCapabilities.SupportsAdaptiveSync) ? PresentInterval.Default : (_verticalSync ? PresentInterval.One : PresentInterval.Immediate);
            presentationParameters.WindowHandle = WindowHandle;

            presentationParameters.MultiSampleCount = graphicsDevice != null
                ? graphicsDevice.GraphicsCapabilities.MaxMultiSampleCount
                : 32;
        }

        private void PrepareGraphicsDeviceInformation(GraphicsDeviceInformation gdi)
        {
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            var pp = new PresentationParameters();
            PreparePresentationParameters(pp);
            gdi.PresentationParameters = pp;
        }

        public void ApplyChanges()
        {
            // If the device hasn't been created then create it now.
            if (graphicsDevice == null)
                CreateDevice();

            if (!_shouldApplyChanges)
                return;

            _shouldApplyChanges = false;

            var gdi = DoPreparingDeviceSettings();
 
            graphicsDevice.Reset(gdi.PresentationParameters);
        }

        private void Initialize(string windowTitle)
        {
            var presentationParameters = new PresentationParameters();
            PreparePresentationParameters(presentationParameters);

            var surfaceFormat = SurfaceFormat.Rgba.GetColorFormat();

            // TODO Need to get this data from the Presentation Parameters
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

            if (SdlGameWindow.Instance != null)
            {
                SdlGameWindow.Instance.CreateWindow(windowTitle);
                WindowHandle = SdlGameWindow.Instance.Handle;
            }

            _initialized = true;
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            ApplyChanges();
        }

        public bool IsFullScreen
        {
            get => _wantFullScreen;
            set
            {
                _shouldApplyChanges = true;
                _wantFullScreen = value;
            }
        }

        public bool HardwareModeSwitch
        {
            get => _hardwareModeSwitch;
            set
            {
                _shouldApplyChanges = true;
                _hardwareModeSwitch = value;
            }
        }

        public int PreferredBackBufferHeight
        {
            get => _preferredBackBufferHeight;
            set
            {
                _shouldApplyChanges = true;
                _preferredBackBufferHeight = value;
            }
        }

        public int PreferredBackBufferWidth
        {
            get => _preferredBackBufferWidth;
            set
            {
                _shouldApplyChanges = true;
                _preferredBackBufferWidth = value;
            }
        }

        public bool AdaptiveSync
        {
            get => _adaptiveSync;
            set
            {
                _shouldApplyChanges = true;
                _adaptiveSync = value;
            }
        }

        public bool VerticalSync
        {
            get => _verticalSync;
            set
            {
                _shouldApplyChanges = true;
                _verticalSync = value;
            }
        }
    }
}
