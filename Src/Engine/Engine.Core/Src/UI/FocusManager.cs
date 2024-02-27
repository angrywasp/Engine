using Engine.UI.Controls;

namespace Engine.UI
{
    public class FocusManager
    {
        public event EngineEventHandler<FocusManager, UiControl> FocusLost;
        public event EngineEventHandler<FocusManager, UiControl> FocusGained;

        private UiControl focusedControl;
        private Interface ui;
        private UiControl focusCandidate = null;

        public FocusManager(Interface ui)
        {
            this.ui = ui;
        }

        public void Update(UiForm mainForm)
        {
            if (!ui.Input.Mouse.LeftJustPressed)
                return;

            focusCandidate = null;
            UpdateInternal(mainForm);
            SetFocus(focusCandidate);
        }

        private void UpdateInternal(UiControl uiControl)
        {
            for (int i = 0; i < uiControl.children.Count; i++)
            {
                var ctrl = uiControl.children[i];

                if (!ctrl.Visible)
                    continue;

                if (!ui.Input.Mouse.InsideRect(ctrl.Bounds))
                    continue;

                if (ctrl.CanFocus)
                {
                    focusCandidate = ctrl;
                    return;
                }

                UpdateInternal(ctrl);
            }
        }

        private void SetFocus(UiControl control)
        {
            if (control != focusedControl)
            {
                if (focusedControl != null)
                {
                    focusedControl.HasFocus = false;
                    FocusLost?.Invoke(this, focusedControl);
                }

                if (control != null)
                {
                    control.HasFocus = true;
                    FocusGained?.Invoke(this, control);
                }
            }

            focusedControl = control;
        }
    }
}