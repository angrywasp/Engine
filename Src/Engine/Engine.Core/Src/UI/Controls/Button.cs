using Microsoft.Xna.Framework;
using System.Numerics;

namespace Engine.UI.Controls
{
    public class Button : UiControl
    {
        private TextBox textBox;

        private string textBoxSkinElementKey;

        private string text;

        public string TextBoxSkinElementKey
        {
            get { return textBoxSkinElementKey; }
            set
            {
                textBoxSkinElementKey = value;

                if (textBox != null)
                    textBox.ApplySkinElement(value);
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;

                if (textBox != null)
                    textBox.Text = value;
            }
        }

        protected internal override bool CanFocus => true;

        public override void Load()
        {
            if (children.Count > 0)
            {
                textBox = (TextBox)this["textBox"];
                textBox.SetPosition(textBox.PercentPosition);
                textBox.SetSize(textBox.PercentSize);
                textBox.TextHorizontalAlignment = TextBox.Align_Mode.Center;
                textBox.Text = text;
            }
            else
            {
                textBox = Add<TextBox>(Vector2.Zero, Vector2.One, "textBox", textBoxSkinElementKey);
                textBox.TextHorizontalAlignment = TextBox.Align_Mode.Center;
                textBox.Text = text;
            }

            base.Load();
        }

        protected override void OnMouseDown(UiControlMouseEventArgs e)
        {
            ApplySkinElementState("Active");
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(UiControlMouseEventArgs e)
        {
            ApplySkinElementState("Over");
            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(UiControlMouseEventArgs e)
        {
            ApplySkinElementState("Over");
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(UiControlMouseEventArgs e)
        {
            ApplySkinElementState("Normal");
            base.OnMouseLeave(e);
        }
    }

    public class ButtonState : ControlState
    {
        public Color BorderColor { get; set; } = new Color(Color.White, 0);

        public float BorderThickness { get; set; } = 0;

        public ButtonState()
        {
        }
    }
}