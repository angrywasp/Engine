using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public abstract class GameBase : IGame, IDisposable
    {
        protected SdlGamePlatform platform;
        protected GraphicsDeviceManager graphicsDeviceManager;
        protected GraphicsDevice graphicsDevice;

        private bool isDisposed;

        public SdlGamePlatform Platform => platform;
        public GraphicsDeviceManager GraphicsDeviceManager => graphicsDeviceManager;
        public GraphicsDevice GraphicsDevice => graphicsDevice;

        public abstract string WindowTitle  { get; }
        
        public GameBase()
        {
            platform = new SdlGamePlatform(this);

            // Calling Update() for first time initializes some systems
            FrameworkDispatcher.Update();

            graphicsDeviceManager = new GraphicsDeviceManager(this);

            graphicsDeviceManager.CreateDevice();
            graphicsDevice = graphicsDeviceManager.GraphicsDevice;
            platform.BeforeInitialize();
            Initialize();
        }

        ~GameBase()
        {
            Dispose(false);
        }

        protected virtual void Initialize()
        {
            ApplyChanges();
        }

        internal void ApplyChanges()
        {
			platform.BeginScreenDeviceChange(graphicsDevice.PresentationParameters.IsFullScreen);
            var viewport = new Viewport(0, 0,
			                            graphicsDevice.PresentationParameters.BackBufferWidth,
			                            graphicsDevice.PresentationParameters.BackBufferHeight);

            graphicsDevice.Viewport = viewport;
			platform.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
        }

        public abstract void Tick();

        public abstract void ExitGame();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            Environment.Exit(0);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (graphicsDeviceManager != null)
                    {
                        graphicsDeviceManager.Dispose();
                        graphicsDeviceManager = null;
                    }

                    if (platform != null)
                    {
                        platform.Dispose();
                        platform = null;
                    }

                    if (SoundEffect._systemState == SoundEffect.SoundSystemState.Initialized)
                    	SoundEffect.PlatformShutdown();
                }

                isDisposed = true;
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        protected void AssertNotDisposed()
        {
            if (isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(
                    name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }
    }
}