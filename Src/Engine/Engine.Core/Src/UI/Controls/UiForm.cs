using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI.Controls
{
    public class UiForm : UiControl
    {
        public override Vector2i PixelSize => ui.Size;
        
        public override Vector2i PixelPosition => Vector2i.Zero;

        public override Rectangle Bounds => new Rectangle(0, 0, ui.Size.X, ui.Size.Y);

        public UiForm(Interface ui)
        {
            this.ui = ui;
            this.form = this;
            this.parent = null;
            this.graphicsDevice = ui.GraphicsDevice;
        }

        public override void CheckNeedDraw()
        {
            drawThisFrame = true;
        }

        public override void Draw()
        {
            ui.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (UiControl control in children)
            {
                control.Draw();
                control.EndDraw();
            }
        }

        internal void Resize()
        {
            foreach (UiControl c in children)
                c.OnResize();
        }
    }
}
