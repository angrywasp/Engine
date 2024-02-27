using Engine;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace GameDemo.UI.Settings
{
    public class SettingsMenu
    {
        protected Button btnGraphics;
		protected Button btnAudio;
		protected Button btnInput;
		protected Button btnNetwork;

        protected Button btnBack;
        protected Button btnApply;

        public string FormFile => "Engine/UI/Default.form";

        public SettingsMenu(EngineCore engine)
        {
        }

        /*protected override void Build()
        {
            Vector2 buttonSize = new Vector2(0.2f, 0.05f);
            
            UiControl ctrlBg = AddControl(Vector2.Zero, Vector2.One, "Background");
            ctrlBg.ApplySkinElement("menuBackground");

            TextBox txtHeading = AddTextBox(Vector2.Zero, new Vector2(0.5f, 0.15f), "Heading", ctrlBg);
            txtHeading.Text = "Settings";
            txtHeading.ApplySkinElement("headingText96");

            btnGraphics = AddButton(new Vector2(0.05f, 0.15f), buttonSize, "Graphics", ctrlBg);
            btnAudio = AddButton(new Vector2(0.05f, 0.25f), buttonSize, "Audio", ctrlBg);
            btnInput = AddButton(new Vector2(0.05f, 0.35f), buttonSize, "Input", ctrlBg);
            btnNetwork = AddButton(new Vector2(0.05f, 0.45f), buttonSize, "Network", ctrlBg);

            btnApply = AddButton(new Vector2(0.05f, 0.8f), buttonSize, "Apply", ctrlBg);
            btnBack = AddButton(new Vector2(0.05f, 0.9f), buttonSize, "Back", ctrlBg);

            btnGraphics.MouseClick += btnGraphics_MouseClick;
            btnAudio.MouseClick += btnAudio_MouseClick;
            btnInput.MouseClick += btnInput_MouseClick;
            btnNetwork.MouseClick += btnNetwork_MouseClick;

            btnApply.MouseClick += btnApply_MouseClick;
            btnBack.MouseClick += btnBack_MouseClick;
        }*/

        private void btnGraphics_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //engine.Interface.ScreenMessages.Write("Not implemented", Color.DarkTurquoise);
        }

        private void btnAudio_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //engine.Interface.ScreenMessages.Write("Not implemented", Color.DarkTurquoise);
        }

        private void btnInput_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //engine.Interface.ScreenMessages.Write("Not implemented", Color.DarkTurquoise);
        }

        private void btnNetwork_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //engine.Interface.ScreenMessages.Write("Not implemented", Color.DarkTurquoise);
        }

        private void btnApply_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            Engine.Configuration.Settings.Save();
        }

        void btnBack_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //BasicGame.Instance.LoadMainMenu();
        }
    }
}