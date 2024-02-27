using System.Numerics;
using Engine;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using GameDemo.Helpers;

namespace GameDemo.UI
{
    public class MainMenu : UiForm
    {
        protected UiControl ctrlFader;
        protected Button btnPlay;
        protected Button btnQuit;
        protected Button btnSettings;
        protected Button btnOnline;

        private EngineCore engine;
        private FadeInTimer timer;

        public MainMenu(EngineCore engine, bool firstLoad) : base(engine.Interface)
        {
            this.engine = engine;
            timer = new FadeInTimer();
            ConstructLayout(firstLoad);
        }

        public void ConstructLayout(bool firstLoad)
        {
            MenuHelper.CreateImageSkinElement(engine, "DemoContent/Textures/Background.texture", "menuBackground");
            MenuHelper.CreateHeadingTextSkinElement(engine, "headingText96", "Engine/Fonts/Default.fontpkg", 96, Color.White);

            this.Control(Vector2.Zero, Vector2.One, skinElement: "menuBackground");
            var t = this.TextBox(new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.15f), text: "Main Menu", skinElement: "headingText96");
            t.DeferText = false;

            ctrlFader = this.Control(Vector2.Zero, Vector2.One, skinElement: "UiControl");

            Vector2 buttonSize = new Vector2(0.2f, 0.05f);

            btnPlay = this.Button(new Vector2(0.05f, 0.25f), buttonSize, text: "Play");
            btnOnline = this.Button(new Vector2(0.05f, 0.35f), buttonSize, text: "Online");
            btnSettings = this.Button(new Vector2(0.05f, 0.45f), buttonSize, text: "Settings");
            btnQuit = this.Button(new Vector2(0.05f, 0.9f), buttonSize, text: "Quit");

            btnPlay.MouseClick += btnPlay_MouseClick;
            btnSettings.MouseClick += btnSettings_MouseClick;
            btnOnline.MouseClick += btnOnline_MouseClick;
            btnQuit.MouseClick += btnQuit_MouseClick;

            if (firstLoad)
            {
                ctrlFader.Visible = true;
                SetFade(255);
            }
            else
                ctrlFader.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!timer.IsFinished)
            {
                SetFade(timer.Fade);
                timer.Update(gameTime);
            }
        }

        public void Show()
        {
            ui.SetMainForm(this);
            timer.Start();
        }

        protected void SetFade(byte p)
        {
            ctrlFader.CurrentState.Background.Color = new Color(0, 0, 0, p);
        }

        private void btnPlay_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            BasicGame.Instance.GameHelper.LoadMap();
        }

        void btnSettings_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //SettingsMenu sm = new SettingsMenu(engine);
            //engine.Interface.ApplyForm(sm, engine.Interface.LoadForm(sm));
        }

        void btnOnline_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //OnlineMenu sm = new OnlineMenu(engine);
            //engine.Interface.ApplyForm(sm, engine.Interface.LoadForm(sm));
        }

        void btnQuit_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            BasicGame.Instance.ExitGame();
        }
    }
}