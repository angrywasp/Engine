
using System;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Editor.Views;
using Engine.Game;
using Microsoft.Xna.Framework;

namespace Engine.Editor
{
    public class EditorGame : DesktopGame
    {
        private static EditorGame instance;
        public static EditorGame Instance => instance;

        private IView view;
        public IView View => view;

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

                if (view.IsInitialized)
                    view.Resize(w, h);

                graphicsDeviceManager.ApplyChanges();
            };

            graphicsDeviceManager.ApplyChanges();

            base.Initialize();

            if (App.Arg != null)
                AssetLoader.Load(App.Arg);
            else
                LoadView(new EmptyView(), null);
        }

        protected override void Update(GameTime gameTime)
        {
            if (view != null && view.IsInitialized)
                view.Update(gameTime);
        }

        public void LoadView(IView v, string f)
        {
            view = v;
            view.Init(f);
        }

        public void UnloadView()
        {
            if (view == null)
                return;

            view.UnInitialize();
        }

        public override void ExitGame()
        {
            if (view != null && view.IsInitialized)
                view.UnInitialize();

            Settings.Save();
            Log.Instance.Shutdown();

            base.ExitGame();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (view != null && view.IsInitialized)
                view.Draw(gameTime);
        }
    }
}
