namespace Engine.UI.Controls
{
    public class RadioButton : CheckBox
    {
        public override bool Checked
        {
            get { return base.Checked; }
            set
            {
                if (value)
                {
                    foreach (var i in Parent)
                        if (i is RadioButton)
                            ((RadioButton)i).Checked = false;
                }

                base.Checked = value;
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (!drawThisFrame)
                return;
        }
    }

    public class RadioButtonState : CheckBoxState
    {
        
    }
}

