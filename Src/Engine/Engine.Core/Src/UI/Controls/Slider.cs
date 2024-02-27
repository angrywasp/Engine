using AngryWasp.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Numerics;

namespace Engine.UI.Controls
{
    public class Slider : UiControl
    {
        public delegate void ValueChangedEventHandler(UiControl sender, SliderValueChangedEventArgs e);
        public event ValueChangedEventHandler ValueChanged;

        public enum Orientation_Mode
        {
            Horizontal,
            Vertical
        }

        private float min = 0;
        private float max = 1;
        private float value = 0.5f;
        private SliderHandle handle;

        private Vector2 endSizePercent = Vector2.Zero;
        private Vector2i endSize = Vector2i.Zero;
        private Rectangle leftEndRect = new Rectangle();
        private Rectangle rightEndRect = new Rectangle();
        private bool dragging = false;
        private bool firstUpdate = true;
        private Orientation_Mode orientation;

        protected internal override bool CanFocus => true;

        public Orientation_Mode Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                firstUpdate = true; //will force an update of the handle position
            }
        }

        public float Minimum
        {
            get { return min; }
            set
            {
                min = value;
                
                if (this.max < value)
                    this.max = value;

                this.value = MathHelper.Clamp(this.value, min, max);
            }
        }

        public float Maximum
        {
            get { return max; }
            set
            {
                max = value;

                if (this.min > value)
                    this.min = value;

                this.value = MathHelper.Clamp(this.value, min, max);
            }
        }

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);

            if (isLoaded)
                CalculateEndRectangles();
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);

            if (isLoaded)
                CalculateEndRectangles();
        }

        public override void SetPosition(Vector2 position)
        {
            base.SetPosition(position);

            if (isLoaded)
                CalculateEndRectangles();
        }

        public override void SetPosition(Vector2i position)
        {
            base.SetPosition(position);

            if (isLoaded)
                CalculateEndRectangles();
        }

        public float Value
        {
            get { return this.value; }
            set
            {
                float temp = MathHelper.Clamp(value, min, max);
                if (this.value != temp)
                {
                    SliderValueChangedEventArgs e = new SliderValueChangedEventArgs
                    {
                        OldValue = this.value,
                        NewValue = temp
                    };

                    this.value = temp;

                    UpdateHandlePosition();

                    if (ValueChanged != null)
                        ValueChanged(this, e);
                }
            }
        }

        public new SliderState CurrentState => (SliderState)currentState; 

        public SliderHandle Handle
        {
            get
            {
                if (handle == null)
                    throw new NullReferenceException("Slider.Handle == null");

                return handle;
            }
        }

        #region SetEndSize

        public void SetEndSize(Vector2 size)
        {
            endSizePercent = size;

            if (size == Vector2.Zero)
                endSize = Vector2i.Zero;
            else
                endSize = new Vector2i((int)(size.X * PixelSize.X), (int)(size.Y * PixelSize.Y));

            if (isLoaded)
                CalculateEndRectangles();
        }

        public void SetEndSize(Vector2i size)
        {
            endSize = size;

            if (size == Vector2i.Zero)
                endSizePercent = Vector2.Zero;
            else
                endSizePercent = new Vector2(size.X / (float)PixelSize.X, size.Y / (float)PixelSize.Y);

            if (isLoaded)
                CalculateEndRectangles();
        }

        #endregion

        #region Mouse Events

        private void hndl_MouseAction(UiControl sender, UiControlMouseEventArgs e) => UpdateMouseInteraction(e.Mouse.ButtonDown(e.Mouse.LeftButton));

        private void form_MouseUp(UiControl sender, UiControlMouseEventArgs e) => UpdateMouseInteraction(false);

        protected override void OnMouseUp(UiControlMouseEventArgs e)
        {
            base.OnMouseUp(e);
            UpdateMouseInteraction(false);
        }

        protected override void OnMouseDown(UiControlMouseEventArgs e)
        {
            base.OnMouseDown(e);
            UpdateMouseInteraction(true);
        }

        protected override void OnMouseLeave(UiControlMouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateMouseInteraction(e.Mouse.ButtonDown(e.Mouse.LeftButton));
        }

        protected override void OnMouseEnter(UiControlMouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateMouseInteraction(e.Mouse.ButtonDown(e.Mouse.LeftButton));
        }

        public void UpdateMouseInteraction(bool down)
        {
            if (down)
            {
                dragging = true;
                handle.MouseClicked = true;
                handle.ApplySkinElementState("Active");
                UpdateHandlePosition();
            }
            else
            {
                dragging = false;
                handle.MouseClicked = false;
                handle.UpdateSkinState();
            }
        }

        #endregion

        public override void Load()
        {
            base.Load();

            if (children.Count > 0)
            {
                handle = (SliderHandle)this["handle"];
                handle.SetPosition(handle.PercentPosition);
                handle.SetSize(handle.PercentSize);
            }
            else
            {
                handle = Add<SliderHandle>(Vector2.Zero, Vector2.One, "handle");
            }

            SetEndSize(endSizePercent);

            handle.MouseDown += new MouseInteractionEventHandler(hndl_MouseAction);
            handle.MouseUp += new MouseInteractionEventHandler(hndl_MouseAction);

            Form.MouseUp += new MouseInteractionEventHandler(form_MouseUp);

            base.Load();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (firstUpdate)
            {
                firstUpdate = false;
                CalculateEndRectangles();
                UpdateHandlePosition();
                return;
            }

            handle.Update(gameTime);

            if (handle.HasFocus && dragging)
            {
                Vector2i pip = PixelPosition;
                Vector2i sip = PixelSize;
                float x = MathHelper.Clamp(ui.Input.Mouse.Position.X, pip.X, pip.X + sip.X);
                float y = MathHelper.Clamp(ui.Input.Mouse.Position.Y, pip.Y, pip.Y + sip.Y);

                switch (Orientation)
                {
                    case Orientation_Mode.Horizontal:
                        Value = MathHelper.Clamp((x - pip.X) * (max / sip.X), min, max);
                        break;
                    case Orientation_Mode.Vertical:
                        Value = MathHelper.Clamp((y - pip.Y) * ((max - min) / sip.Y), min, max);
                        break;
                }

                UpdateHandlePosition();
            }
        }

        public void UpdateHandlePosition()
        {
            Vector2i sip = PixelSize;
            Vector2i hsip = handle.PixelSize;

            //todo: can we just update x and y here
            switch (Orientation)
            {
                case Orientation_Mode.Horizontal:
                    {
                        float val = (value - min) / (max - min);
                        handle.SetPosition(new Vector2i((int)(val * sip.X - (hsip.X / 2)), (int)(-(hsip.Y / 2) + (sip.Y / 2))));
                    }
                    break;
                case Orientation_Mode.Vertical:
                    {
                        float val = (value - min) / (max - min);
                        handle.SetPosition(new Vector2i((int)(-(hsip.X / 2) + (sip.X / 2)), (int)(val * sip.Y - (hsip.Y / 2))));
                    }
                    break;
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;

            int w = endSize.X;
            int h = endSize.Y;

            if (w > 0 && h > 0)
            {
                TextureRegion ne = CurrentState.NearEnd;
                TextureRegion fe = CurrentState.FarEnd;

                if (ne.HasTexture)
                    ui.DrawRectangle(ne.Texture, Color.White, new Vector2i(leftEndRect.X, leftEndRect.Y), new Vector2i(leftEndRect.Width, leftEndRect.Height), ne.Region);
                else
                    ui.DrawRectangle(ne.Color, new Vector2i(leftEndRect.X, leftEndRect.Y), new Vector2i(leftEndRect.Width, leftEndRect.Height));

                if (fe.HasTexture)
                    ui.DrawRectangle(fe.Texture, Color.White, new Vector2i(rightEndRect.X, rightEndRect.Y), new Vector2i(rightEndRect.Width, rightEndRect.Height), fe.Region);
                else
                    ui.DrawRectangle(fe.Color, new Vector2i(rightEndRect.X, rightEndRect.Y), new Vector2i(rightEndRect.Width, rightEndRect.Height));
            }

            handle.Draw();
        }

        internal override void OnResize()
        {
            firstUpdate = true;
            base.OnResize();
        }

        private void CalculateEndRectangles()
        {
            //todo: only do this when the size or position of the control changes, or if the size of the form changes
            //draws the ends of the bar
            switch (Orientation)
            {
                case Orientation_Mode.Horizontal:
                    {
                        int sx = endSize.X;
                        int sy = endSize.Y;
                        int y = (int)(PixelPosition.Y - (sy / 2) + (PixelSize.Y / 2));

                        leftEndRect.X = (int)(PixelPosition.X - sx);
                        leftEndRect.Y = y;
                        leftEndRect.Width = sx;
                        leftEndRect.Height = sy;

                        rightEndRect.X = (int)(PixelPosition.X + PixelSize.X);
                        rightEndRect.Y = y;
                        rightEndRect.Width = sx;
                        rightEndRect.Height = sy;
                    }
                    break;

                //todo: needs work.
                case Orientation_Mode.Vertical:
                    {
                        int sx = endSize.X;
                        int sy = endSize.Y;
                        int x = (int)(PixelPosition.X + (PixelSize.X / 2) - (sx / 2));

                        leftEndRect.X = x;
                        leftEndRect.Y = (int)(PixelPosition.Y - sy);
                        leftEndRect.Width = sx;
                        leftEndRect.Height = sy;

                        rightEndRect.X = x;
                        rightEndRect.Y = (int)(PixelPosition.Y + PixelSize.Y);
                        rightEndRect.Width = sx;
                        rightEndRect.Height = sy;
                    }
                    break;
            }
        }
    }

    public class SliderState : ControlState
    {
        public TextureRegion NearEnd { get; set; } = new TextureRegion();

        public TextureRegion FarEnd { get; set; } = new TextureRegion();

        public override void Load(Interface ui)
        {
            base.Load(ui);
            NearEnd.Load(ui);
            FarEnd.Load(ui);
        }
    }

    public class SliderValueChangedEventArgs
    {
        public float OldValue { get; set; } = 0.0f;
        public float NewValue { get; set; } = 0.0f;    
    }
}