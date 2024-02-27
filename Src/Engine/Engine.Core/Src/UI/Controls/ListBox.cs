using AngryWasp.Helpers;

namespace Engine.UI.Controls
{
    public class ListBox : ListBoxBase
    {
        public event EngineEventHandler<ListBox, (int Index, ListBoxItem Item)> SelectedItemChanged;

        private int selectedIndex = -1;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                value = MathHelper.Clamp(value, -1, items.Count - 1);

                bool valueChanged = false;

                if (selectedIndex != value)
                    valueChanged = true;

                selectedIndex = value;

                if (valueChanged)
                    SelectedItemChanged?.Invoke(this, (value, value == -1 ? null : items[value]));
            }
        }

        public ListBoxItem SelectedItem => selectedIndex == -1 ? null : items[selectedIndex];

        public override void Clear() => selectedIndex = -1;

        protected override ListBoxItemState ResolveItemState(int x)
        {
            ListBoxItemState s = CurrentState.Item;

            if (x == selectedIndex)
                s = CurrentState.ItemSelected;
            else if (x == hoverIndex)
                s = CurrentState.ItemHover;

            return s;
        }

        protected override void OnMouseClick(UiControlMouseEventArgs e)
        {
            selectedIndex = hoverIndex;
            if (SelectedItemChanged != null && selectedIndex >= 0 && selectedIndex < items.Count)
                SelectedItemChanged(this, (selectedIndex, items[selectedIndex]));
            base.OnMouseClick(e);
        }
    }
}
