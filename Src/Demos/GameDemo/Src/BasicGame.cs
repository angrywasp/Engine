using System;
using Engine.Configuration;
using Microsoft.Xna.Framework;
using GameDemo.Helpers;
using AngryWasp.Logger;
using Engine;
using System.IO;
using System.Reflection;
using GameDemo.UI;
using GameDemo.UI.Intro;
using Engine.Game;

namespace GameDemo
{
    public class BasicGame : DesktopGame
    {
        private static BasicGame instance;

        public static BasicGame Instance => instance;

        private bool skipIntro = true;
        private EngineCore engine;
        private GameHelper gameHelper;
        private IView view = new ViewImpl();

        public GameHelper GameHelper => gameHelper;

        protected override void Initialize()
        {
            instance = this;

            this.TargetFPS = Settings.Engine.TargetFPS;

            graphicsDeviceManager.PreferredBackBufferWidth = Settings.Engine.Resolution.X;
            graphicsDeviceManager.PreferredBackBufferHeight = Settings.Engine.Resolution.Y;
            graphicsDeviceManager.AdaptiveSync = Settings.Engine.AdaptiveSync;
            graphicsDeviceManager.VerticalSync = Settings.Engine.VerticalSync;

            this.IsMouseVisible = true;

            if (Settings.Engine.FullScreen)
            {
                Window.IsBorderless = true;
                Window.AllowUserResizing = false;
                Window.Position = new Microsoft.Xna.Framework.Point(50, 50);
                graphicsDeviceManager.IsFullScreen = false;
            }
            else
            {
                Window.IsBorderless = false;
                Window.Position = new Microsoft.Xna.Framework.Point(50, 50);
                Window.AllowUserResizing = true;
            }

            Window.ClientSizeChanged += (object sender, EventArgs e) =>
            {
                int w = Window.ClientBounds.Width;
                int h = Window.ClientBounds.Height;

                graphicsDeviceManager.PreferredBackBufferWidth = w;
                graphicsDeviceManager.PreferredBackBufferHeight = h;
                //graphicsDeviceManager.ApplyChanges();

                engine.Resize(w, h);
                view.Resize(w, h);

                graphicsDeviceManager.ApplyChanges();
            };

            graphicsDeviceManager.ApplyChanges();

            string typeAssemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Engine.Objects.dll");
            engine = new EngineCore(GraphicsDevice, typeAssemblyPath);

            engine.Scene.Graphics.DebugDraw = false;
            engine.Scene.Physics.SimulationUpdate = true;

            gameHelper = new GameHelper(engine);

            base.Initialize();

            engine.Resize(Settings.Engine.Resolution.X, Settings.Engine.Resolution.Y);

            var mainMenu = new MainMenu(engine, true);

            if (skipIntro)
                mainMenu.Show();
            else
                new IntroLoader().Run(engine, mainMenu);
        }

        protected override void Update(GameTime gameTime)
        {
            engine.Update(gameTime);

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.OemTilde))
            {
                engine.Interface.Terminal.ResetCommandHistory();
                engine.Interface.Terminal.Visible = !engine.Interface.Terminal.Visible;
            }

            if (!view.ShouldUpdate())
                return;

            view.Update(gameTime);
            engine.Scene.OfflineGameUpdate(engine.Camera, gameTime);

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
                engine.Scene.Graphics.DebugDraw = !engine.Scene.Graphics.DebugDraw;
        }

        public void LoadView(IView v)
        {
            v.Initialize();
            view = v;
        }

        public void UnloadView()
        {
            view.UnInitialize();
        }

        public override void ExitGame()
        {
            view.UnInitialize();
            engine.Shutdown();

            Settings.Save();
            Log.Instance.Shutdown();

            base.ExitGame();
        }

        protected override void Draw(GameTime gameTime)
        {
            FPSCounter.Update(gameTime);
            view.Draw(gameTime);
            engine.Scene.Draw(engine.Camera, gameTime);
            engine.Draw(engine.Camera, gameTime);
            engine.Interface.ScreenMessages.WriteStaticText(0, $"FPS: {FPSCounter.FramePerSecond}", FPSCounter.FramePerSecond < Settings.Engine.TargetFPS ? Color.Red : Color.Green);
        }
    }
}
