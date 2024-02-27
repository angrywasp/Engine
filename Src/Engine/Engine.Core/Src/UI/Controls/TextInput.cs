using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static Engine.UI.UIRenderer;

namespace Engine.UI.Controls
{
    public class TextInput : UiControl
    {
        public delegate void TextChangedEventHandler(TextInput sender, string oldValue, string newValue);

        public event TextChangedEventHandler TextChanged;

        private string text;
        private string lastText;

        //the character index within the string the caret is positioned at
        private int caretCharPosition = 0;
        //the x pixel value of the position to draw the caret
        private int caretDrawPosition = 0;
        private Vector2i textSize = Vector2i.Zero;
        private int textIndent = 10;
        private float elapsedFlashTime = 0;
        private bool caretOn;

        public string Text
        {
            get { return text; }
            set
            {
                string oldValue = text;
                string newValue = value;
                text = lastText = value;
                caretCharPosition = value.Length;
                UpdateTextMeasurements();

                if (oldValue != newValue)
                    TextChanged?.Invoke(this, oldValue, newValue);
            }
        }

        public bool DisplayAsPassword { get; set; } = false;

        public new TextInputState CurrentState => (TextInputState)currentState;

        protected internal override bool CanFocus => true;

        public override void Load()
        {
            base.Load();

            caretCharPosition = text != null ? text.Length : 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            elapsedFlashTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (CurrentState == null)
                return;

            if (elapsedFlashTime > CurrentState.CaretFlashSpeed)
            {
                caretOn = !caretOn;
                elapsedFlashTime = 0;
            }

            if (HasFocus)
            {
                int adjustedPos;
                string s2 = text;

                ui.Input.Keyboard.HandleKeyboardTextInput(ref s2, caretCharPosition, out adjustedPos, true);
                text = s2;
                caretCharPosition = adjustedPos;

                if (string.IsNullOrEmpty(text))
                {
                    caretDrawPosition = 0;
                    return;
                }
                else
                    UpdateTextMeasurements();

                if (lastText != text)
                {
                    lastText = text;
                    TextChanged?.Invoke(this, lastText, text);
                }
            }
        }

        private void UpdateTextMeasurements()
        {
            if (string.IsNullOrEmpty(text) || CurrentState == null)
            {
                textSize = Vector2i.Zero;
                caretDrawPosition = 0;
                return;
            }

            textSize = CurrentState.Font.MeasureString(text);
            caretDrawPosition = CurrentState.Font.MeasureString(text.Substring(0, caretCharPosition)).X;
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;

            //we need to handle when we go back with the arrow. to shift the text so the caret is visible
            int caretWidth = 1;

            Vector2i pip = PixelPosition;
            Vector2i sip = PixelSize;
            pip.X += textIndent;

            int visibleTextArea = (sip.X - textIndent * 2);
            Rectangle textScissor = new Rectangle(pip.X, pip.Y, visibleTextArea + caretWidth, sip.Y);

            if (textSize.X > visibleTextArea)
                pip.X -= (textSize.X - visibleTextArea); 

            graphicsDevice.ScissorRectangle = textScissor;

            Dictionary<int, DeferredText> deferredText = new Dictionary<int, DeferredText> ();
            VertexPositionColorTexture[] vertices;
            int[] indices;

            if (!string.IsNullOrEmpty(text))
            {
                ui.DrawString(text, new Vector2i (pip.X, pip.Y + (sip.Y - textSize.Y) / 2), CurrentState.Font, CurrentState.TextColor, out vertices, out indices);
                ui.DeferText(deferredText, vertices, indices, CurrentState.Font.Texture);
                ui.DrawDeferredText(deferredText);
            }

            if (caretOn && HasFocus)
            {
                pip.X += caretDrawPosition;
                ui.DrawRectangle(CurrentState.CaretColor, pip, new Vector2i(caretWidth, sip.Y));
            }

            graphicsDevice.ScissorRectangle = graphicsDevice.Viewport.Bounds;
        }

        public override void ApplySkinElement(string elementKey)
        {
            base.ApplySkinElement(elementKey);
            UpdateTextMeasurements();
        }

        public override void ApplySkinElementState(string key)
        {
            base.ApplySkinElementState(key);
            UpdateTextMeasurements();
        }
    }

    public class TextInputState : TextBoxState
    {
        public char PasswordChar { get; set; } = '*';
        public float CaretFlashSpeed { get; set; } = 0.5f;
        public Color CaretColor { get; set; } = Color.Red;
    }
}