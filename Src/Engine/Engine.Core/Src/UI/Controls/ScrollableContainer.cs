using AngryWasp.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Numerics;

namespace Engine.UI.Controls
{
    public class ScrollableContainer : UiControl
    {
        public const int SLIDER_WIDTH = 10;
        private Slider verticalSlider;
        private Slider horizontalSlider;

        private UiControl contentArea;

         private UiControl content;

        Rectangle contentScissorRectangle = Rectangle.Empty;

        public Slider HorizontalSlider
        {
            get
            {
                if (horizontalSlider == null)
                    throw new NullReferenceException("ScrollableContainer.HorizontalSlider == null");

                return horizontalSlider;
            }
        }

        public Slider VerticalSlider
        {
            get
            {
                if (verticalSlider == null)
                    throw new NullReferenceException("ScrollableContainer.VerticalSlider == null");

                return verticalSlider;
            }
        }

        public UiControl ContentArea
        {
            get
            {
                if (contentArea == null)
                    throw new NullReferenceException("ScrollableContainer.ContentArea == null");

                return contentArea;
            }
        }

        private void CalibrateInternalControls()
        {
            if (this.PixelSize == Vector2i.Zero)
                return;

            if (verticalSlider == null || horizontalSlider == null || contentArea == null || content == null)
                return;

            //first we need to determine if the sliders will be visible
            if (content != null && content.PixelSize.Y <= contentArea.PixelSize.Y)
                verticalSlider.Visible = false;

            if (content != null && content.PixelSize.X <= contentArea.PixelSize.X)
                horizontalSlider.Visible = false;

            int px, py;
            int sx, sy;

            if (verticalSlider.Visible)
            {
                px = this.PixelSize.X - SLIDER_WIDTH;
                py = SLIDER_WIDTH / 2;

                sx = SLIDER_WIDTH;
                sy = this.PixelSize.Y - (horizontalSlider.Visible ? (SLIDER_WIDTH * 2) : SLIDER_WIDTH);

                verticalSlider.SetPosition(new Vector2i(px, py));
                verticalSlider.SetSize(new Vector2i(sx, sy));
                verticalSlider.Handle.SetSize(new Vector2i(SLIDER_WIDTH, SLIDER_WIDTH));
                verticalSlider.SetEndSize(new Vector2i(SLIDER_WIDTH, SLIDER_WIDTH / 2));
            }

            if (horizontalSlider.Visible)
            {
                px = SLIDER_WIDTH / 2;
                py = this.PixelSize.Y - SLIDER_WIDTH;

                sx = this.PixelSize.X - (verticalSlider.Visible ? (SLIDER_WIDTH * 2) : SLIDER_WIDTH);
                sy = SLIDER_WIDTH;

                horizontalSlider.SetPosition(new Vector2i(px, py));
                horizontalSlider.SetSize(new Vector2i(sx, sy));
                horizontalSlider.Handle.SetSize(new Vector2i(SLIDER_WIDTH, SLIDER_WIDTH));
                horizontalSlider.SetEndSize(new Vector2i(SLIDER_WIDTH / 2, SLIDER_WIDTH));
            }

            contentArea.SetPosition(Vector2.Zero);
            contentArea.SetSize(new Vector2i(this.PixelSize.X - (verticalSlider.Visible ? SLIDER_WIDTH : 0), this.PixelSize.Y - (horizontalSlider.Visible ? SLIDER_WIDTH : 0)));
            contentScissorRectangle = contentArea.Bounds;

            if (content != null)
            {
                int x = content.PixelPosition.X - contentArea.PixelPosition.X;
                horizontalSlider.Minimum = 0f;
                horizontalSlider.Maximum = content.PixelSize.X - contentArea.PixelSize.X;
                horizontalSlider.Value = x;

                int y = content.PixelPosition.Y - contentArea.PixelPosition.Y;
                verticalSlider.Minimum = 0f;
                verticalSlider.Maximum = content.PixelSize.Y - contentArea.PixelSize.Y;
                verticalSlider.Value = y;
            }

            verticalSlider.ValueChanged += (s, e) =>
                content.SetPosition(new Vector2i(0, -(int)verticalSlider.Value));

            horizontalSlider.ValueChanged += (s, e) =>
                content.SetPosition(new Vector2i(-(int)horizontalSlider.Value, content.PixelPosition.Y));
        }

        internal void SetContent(UiControl content)
        {
            this.content = content;
            CalibrateInternalControls();
        }

        public override void Load()
        {
            if (isLoaded)
                return;
                
            horizontalSlider = this.Slider(Vector2.Zero, Vector2.Zero, "hSlider");
            horizontalSlider.Orientation = Slider.Orientation_Mode.Horizontal;

            verticalSlider = this.Slider(Vector2.Zero, Vector2.Zero, "vSlider");
            verticalSlider.Orientation = Slider.Orientation_Mode.Vertical;

            contentArea = this.Control(Vector2.Zero, Vector2.One, "contentArea", "BlankControl");

            CalibrateInternalControls();

            Form.MouseUp += delegate(UiControl sender, UiControlMouseEventArgs e)
            {
                verticalSlider.UpdateMouseInteraction(false);
                horizontalSlider.UpdateMouseInteraction(false);
            };

            base.Load();
        }

        public override void Update(GameTime gameTime)
        {
            verticalSlider.Update(gameTime);
            horizontalSlider.Update(gameTime);

            base.Update(gameTime);
        }

        public override void CheckNeedDraw()
        {
            if (!Visible)
                drawThisFrame = false;

            if (PixelSize.X == 0 || PixelSize.Y == 0 || (Bounds.Width == 0 || Bounds.Height == 0))
                drawThisFrame = false;

            if (content == null || !content.Visible)
                drawThisFrame = false;
        }

        public override void Draw()
        {
            CheckNeedDraw();
            if (!drawThisFrame)
                return;

            Rectangle b = Bounds;
            content.ScissorRectangle = contentScissorRectangle;

            foreach (UiControl control in children)
                control.Draw();

            graphicsDevice.ScissorRectangle = graphicsDevice.Viewport.Bounds;
        }

        internal override void OnResize()
        {
            pixPosition = PositionToPixel(perPosition);
            pixSize = SizeToPixel(perSize);
            CalibrateInternalControls();

            if (content != null)
                content.OnResize();
        }

        public override void SetPosition(Vector2 position)
        {
            base.SetPosition(position);
            CalibrateInternalControls();
        }

        public override void SetPosition(Vector2i position)
        {
            base.SetPosition(position);
            CalibrateInternalControls();
        }

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            CalibrateInternalControls();
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);
            CalibrateInternalControls();
        }

        private int scrollDelta = 25;

        protected override void OnMouseScroll(UiControlMouseEventArgs e)
        {
            base.OnMouseScroll(e);

            if (content == null || !content.Visible)
                return;

            int y = (int)verticalSlider.Value;

            if (e.Mouse.ScrollDelta > 0)
                y -= scrollDelta;
            else if (e.Mouse.ScrollDelta < 0)
                y += scrollDelta;
            
            y = MathHelper.Clamp(y, (int)verticalSlider.Minimum, (int)verticalSlider.Maximum);
            verticalSlider.Value = y;
        }
    }

    public class ScrollableContainerState : ControlState
    {

    }
}
