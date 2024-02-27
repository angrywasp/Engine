using Engine.UI.Controls;
using System.Collections.Generic;

namespace Engine.UI
{
    public class SkinElement
    {
        private Interface ui;

        private Dictionary<string, ControlState> states = new Dictionary<string, ControlState>();

        public Dictionary<string, ControlState> States => states;

        public SkinElement() { }

        public void Load(Interface ui)
        {
            this.ui = ui;

            foreach(var s in states)
                s.Value.Load(ui);
        }
    }
}