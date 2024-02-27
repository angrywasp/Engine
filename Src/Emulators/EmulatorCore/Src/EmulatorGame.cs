using System;
using Engine.Configuration;
using Microsoft.Xna.Framework;
using Engine.Game;
using Engine.UI;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;

namespace EmulatorCore
{
    public abstract class EmulatorGame : DesktopGame
	{
		protected InputDeviceManager input;
        protected Interface ui;
        protected UIRenderer uiRenderer;
        protected bool running = false;
        protected RenderTarget2D rt;
        
        protected override void Initialize()
        {
            this.TargetFPS = Settings.Engine.TargetFPS;
            
            graphicsDeviceManager = new GraphicsDeviceManager(this);
			graphicsDeviceManager.PreferredBackBufferWidth = Settings.Engine.Resolution.X;
			graphicsDeviceManager.PreferredBackBufferHeight = Settings.Engine.Resolution.Y;
            graphicsDeviceManager.AdaptiveSync = Settings.Engine.AdaptiveSync;
            graphicsDeviceManager.VerticalSync = Settings.Engine.VerticalSync;

            if (Settings.Engine.FullScreen)
            {
                Window.IsBorderless = true;
                Window.Position = new Point(0, 0);
                graphicsDeviceManager.IsFullScreen = true;
                graphicsDeviceManager.ApplyChanges();
            }
            else
            {
                Window.IsBorderless = false;
            }

            Window.ClientSizeChanged += (object sender, EventArgs e) => 
                {
                    int w = Window.ClientBounds.Width;
                    int h = Window.ClientBounds.Height;

                    Viewport viewport = new Viewport(0, 0, w, h);

                    graphicsDevice.Viewport = viewport;
                    PresentationParameters pp = graphicsDevice.PresentationParameters;
                    pp.BackBufferWidth = w;
                    pp.BackBufferHeight = h;
                    graphicsDevice.Reset(pp);

                    ui.Resize(0, 0, w, h);
                    uiRenderer.SetBufferSize();
                };

            input = new InputDeviceManager(graphicsDevice);
            ui = new Interface(graphicsDevice, input);
            uiRenderer = new UIRenderer(graphicsDevice);

            ui.Resize(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            uiRenderer.SetBufferSize();

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
            input.Update(gameTime);
            ui.Update(gameTime);

            if (ui.Terminal.Visible)
                running = false;
            else
                running = true;

            if (input.Keyboard.KeyJustPressed(Keys.OemTilde))
            {
                ui.Terminal.ResetCommandHistory();
                ui.Terminal.Visible = !ui.Terminal.Visible;
            }
		}

        public override void ExitGame()
        {
            Settings.Save();
            base.ExitGame();
        }
	}
}
