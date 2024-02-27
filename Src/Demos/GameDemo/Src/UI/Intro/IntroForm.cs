using Engine;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using GameDemo.Helpers;

namespace GameDemo.UI.Intro
{
    public abstract class IntroForm : UiForm
    {
        protected UiControl ctrlFader;
        protected EngineCore engine;

        private Timer timer;
        private IntroLoader introLoader;

        public IntroForm(EngineCore engine, IntroLoader introLoader) : base(engine.Interface)
        {
            this.engine = engine;
            this.introLoader = introLoader;
            ConstructLayout();
        }

        protected abstract void ConstructLayout();

        protected void SetFade(byte p)
        {
            ctrlFader.CurrentState.Background.Color = new Color(0, 0, 0, p);
        }

        protected virtual void TimerFinished()
        {
            introLoader.LoadNextForm();
        }

        public void Show()
        {
            ui.SetMainForm(this);
            timer = new Timer();
            timer.Finished += delegate(Timer sender)
            {
                form.Visible = false;
                TimerFinished();
            };

            timer.Start();
            SetFade(255);
        }

        public override void Update(GameTime gameTime)
        {
            SetFade(timer.Fade);
            timer.Update(gameTime);
        }
    }
}