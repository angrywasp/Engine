using Engine.Content;
using Microsoft.Xna.Framework;
using System.Numerics;

namespace Engine.UI.Controls
{
    public class TextBox : UiControl
    {
        public enum Align_Mode
        {
            Center,
            Near,
            Far
        }

        public enum Text_Crop_Mode
        {
            /// <summary>
            /// no cropping of text
            /// </summary>
            None,

            /// <summary>
            /// crops the text to the bounds of this textbox
            /// </summary>
            ToSelf,

            /// <summary>
            /// crops the text to the immediate parent bounds. preferred method if autosize == true
            /// </summary>
            ToParent
        }

        public new TextBoxState CurrentState => (TextBoxState)currentState;

        private bool autoSize = false;
        protected Vector2 textSize;
        protected int textIndent = 10;
        private string text;

        public bool AutoSize
        {
            get { return autoSize; }
            set
            {
                autoSize = value;

                if (autoSize)
                    SetSizeToText();
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;

                    if (autoSize)
                        SetSizeToText();
                }
            }
        }

        public int TextIndent => textIndent;

        public Align_Mode TextHorizontalAlignment { get; set; } = Align_Mode.Near;

        public Align_Mode TextVerticalAlignment { get; set; } = Align_Mode.Center;

        public Text_Crop_Mode TextCropMode { get; set; } = Text_Crop_Mode.ToSelf;

        public bool DeferText { get; set; } = true;

        public void SetSizeToText(string s = null)
        {
            string t = s != null ? s : Text;
            //we default to Pixel dimensioning here cause MeasureString gives us a size in Pixels and it isn't worth the bother to do any conversions here
            if (CurrentState == null || CurrentState.Font == null || string.IsNullOrEmpty(t))
            {
                pixSize = Vector2i.Zero;
                return;
            }

            Vector2i textSize = CurrentState.Font.MeasureString(t) + new Vector2i(textIndent * 2, 0);
            pixSize = textSize;
            return;
        }

        public override void CheckNeedDraw()
        {
            base.CheckNeedDraw();
            if (!drawThisFrame)
                return;

            if (CurrentState == null || CurrentState.Font == null)
                drawThisFrame = false;
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;

            if (!string.IsNullOrEmpty(Text))
            {
                textSize = CurrentState.Font.MeasureString(Text).ToVector2();
                Vector2i textOffset = Vector2i.Zero;
                Vector2 sx = PixelSize.ToVector2();
                switch (TextHorizontalAlignment)
                {
                    case Align_Mode.Near:
                        textOffset.X = textIndent;
                        break;

                    case Align_Mode.Center:
                        textOffset.X = (int)((sx.X / 2.0f) - (textSize.X / 2.0f));
                        break;

                    case Align_Mode.Far:
                        textOffset.X = (int)(sx.X - (textSize.X + textIndent));
                        break;
                }

                switch (TextVerticalAlignment)
                {
                    case Align_Mode.Near:
                        textOffset.Y = textIndent;
                        break;

                    case Align_Mode.Center:
                        textOffset.Y = (int)((sx.Y / 2.0f) - (textSize.Y / 2.0f));
                        break;

                    case Align_Mode.Far:
                        textOffset.Y = (int)(sx.Y - (textSize.Y + textIndent));
                        break;
                }

                Vector2i textPosition = new Vector2i(Bounds.X + textOffset.X, Bounds.Y + textOffset.Y);

                if (TextCropMode != Text_Crop_Mode.None)
                {
                    Rectangle r = GetVisibleBounds();
                    r.X += (int)textIndent;
                    r.Width -= (int)(textIndent * 2);

                    graphicsDevice.ScissorRectangle = r;
                }

                ui.DrawString(Text, textPosition, CurrentState.Font, CurrentState.TextColor);
                if (!DeferText)
                    ui.DrawDeferredText();
            }
            graphicsDevice.ScissorRectangle = graphicsDevice.Viewport.Bounds;
        }

        public Vector2i GetSizeOfChar(int index)
        {
            if (CurrentState == null || CurrentState.Font == null)
                return Vector2i.Zero;

            if (index >= Text.Length)
                return Vector2i.Zero;

            return CurrentState.Font.MeasureString(Text[index].ToString());
        }

        public Vector2i GetSizeOfChar(char c)
        {
            if (CurrentState == null || CurrentState.Font == null)
                return Vector2i.Zero;

            return CurrentState.Font.MeasureString(c.ToString());
        }

        public Vector2i GetSizeOfString(string s)
        {
            if (CurrentState == null || CurrentState.Font == null)
                return Vector2i.Zero;

            return CurrentState.Font.MeasureString(s);
        }

        public Rectangle GetVisibleBounds()
        {
            Rectangle r;
            switch (TextCropMode)
            {
                case Text_Crop_Mode.ToParent:
                    r = Parent.Bounds;
                    break;

                default:
                    r = Bounds;
                    break;
            }

            return r;
        }
    }

    public class TextBoxState : ControlState
    {
        private Font font;

        public Font Font => font;

        public string FontPath { get; set; }

        public int FontSize { get; set; } = -1;

        public Color TextColor { get; set; } = new Color(Color.White, 0);

        public bool HasFont => Font != null;

        public override void Load(Interface ui)
        {
            base.Load(ui);

            var fontPackage = !string.IsNullOrEmpty(FontPath) ?
                ContentLoader.LoadFontPackage(ui.GraphicsDevice, FontPath) :
                ui.DefaultFont;

            font = FontSize == -1 ? fontPackage.Smallest() : fontPackage.GetByFontSize(FontSize);
        }
    }
}