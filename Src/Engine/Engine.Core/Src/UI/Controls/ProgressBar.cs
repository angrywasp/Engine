using System.Diagnostics;
using System.Numerics;
using AngryWasp.Helpers;
using Microsoft.Xna.Framework;

namespace Engine.UI.Controls
{
    public class ProgressBar : UiControl
    {
        public enum Orientation_Mode
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop
        }

        private Vector2 foreTextureScale = new Vector2(1, 1);
        private Rectangle rect = Rectangle.Empty;
        private float min = 0;
        private float max = 1;
        private float value = 0.5f;

        public Orientation_Mode Orientation { get; set; }

        public float Minimum
        {
            get { return min; }
            set
            {
                min = MathHelper.Clamp(value, min, this.value);
            }
        }

        public float Maximum
        {
            get { return max; }
            set
            {
                max = MathHelper.Clamp(value, this.value, value);
            }
        }

        public float Value
        {
            get { return this.value; }
            set
            {
                this.value = MathHelper.Clamp(value, min, max);
            }
        }

        public new ProgressBarState CurrentState
        {
            get { return (ProgressBarState)currentState; }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Rectangle b = Bounds;

            switch (Orientation)
            {
                case Orientation_Mode.LeftToRight:
                    {
                        rect.Height = b.Height;
                        rect.Width = (int)((value / (Maximum - Minimum)) * b.Width);
                        rect.X = b.X;
                        rect.Y = b.Y;
                    }
                    break;

                case Orientation_Mode.RightToLeft:
                    {
                        rect.Height = b.Height;
                        rect.Width = (int)((value / (Maximum - Minimum)) * b.Width);
                        rect.X = b.X + (b.Width - rect.Width);
                        rect.Y = b.Y;
                    }
                    break;

                case Orientation_Mode.TopToBottom:
                    {
                        rect.Width = b.Width;
                        rect.Height = (int)((value / (Maximum - Minimum)) * b.Height);
                        rect.X = b.X;
                        rect.Y = b.Y;
                    }
                    break;

                case Orientation_Mode.BottomToTop:
                    {
                        rect.Width = b.Width;
                        rect.Height = (int)((value / (Maximum - Minimum)) * b.Height);
                        rect.X = b.X;
                        rect.Y = b.Y + (b.Height - rect.Height);
                    }
                    break;
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;

            //basically the same as the base texture implementation, but for
            //the foreground texture to be placed over the top to represent the value
            Rectangle b = Bounds;
            Rectangle destinationRectangle = new Rectangle((int)b.X, (int)b.Y, b.Width, b.Height);

            TextureRegion fg = CurrentState.Foreground;

            if (fg.HasTexture)
            {
                if (!fg.Wrap)
                    ui.DrawRectangle(fg.Texture, fg.Color, PixelPosition, PixelSize, fg.Region);
                else
                    Debugger.Break(); //todo: fix. this code worked before changing texture regions from RectangleF to Rectangle
            }
            else
                ui.DrawRectangle(fg.Color, new Vector2i(rect.X, rect.Y), new Vector2i(rect.Width, rect.Height));
        }
    }

    public class ProgressBarState : ControlState
    {
        public TextureRegion Foreground { get; set; } = new TextureRegion();

        public override void Load(Interface ui)
        {
            base.Load(ui);
            Foreground.Load(ui);
        }
    }
}