using System.Collections.Generic;

namespace Engine.UI.Controls
{
    public class CheckBox : Button
    {
        public delegate void CheckChangedEventHandler(CheckBox sender, bool isChecked);

        public event CheckChangedEventHandler CheckChanged;

        private bool isChk;
        private string mouseStateKey;

        public virtual bool Checked
        {
            get { return isChk; }
            set
            {
                isChk = value;
                //re-apply the mouse state key to updated the checked state of the skin element
                ApplySkinElementState(mouseStateKey);

                CheckChanged?.Invoke(this, Checked);
            }
        }

        public override List<string> SupportedStates
        {
            get
            {
                List<string> s = base.SupportedStates;
                s.AddRange(new string[] { "CheckedNormal", "CheckedOver", "CheckedActive", "CheckedDisabled" });
                return s;
            }
        }

        protected override void OnMouseClick(UiControlMouseEventArgs e)
        {
            base.OnMouseClick(e);
            Checked = !Checked;
        }

        public override void ApplySkinElementState(string key)
        {
            mouseStateKey = key;

            key = isChk ? "Checked" + key : key;
            base.ApplySkinElementState(key);
        }
    }

    public class CheckBoxState : ButtonState
    {
        
    }
}