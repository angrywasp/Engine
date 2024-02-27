namespace Engine.UI.Controls
{
    public class SliderHandle : UiControl
    {
        private bool mouseClicked = false;

        public bool MouseClicked
        {
            get { return mouseClicked; }
            set { mouseClicked = value; }
        }
        
        public void UpdateSkinState()
        {
            if (ui.Input.Mouse.InsideRect(Bounds))
                ApplySkinElementState("Over");
            else
                ApplySkinElementState("Normal");
        }

        protected override void OnMouseDown(UiControlMouseEventArgs e)
        {
            ApplySkinElementState("Active");
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(UiControlMouseEventArgs e)
        {
            if (!mouseClicked)
                ApplySkinElementState("Over");

            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(UiControlMouseEventArgs e)
        {
            if (!mouseClicked)
                ApplySkinElementState("Over");

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(UiControlMouseEventArgs e)
        {
            if (!mouseClicked)
                ApplySkinElementState("Normal");

            base.OnMouseLeave(e);
        }
    }
}
