using System.Collections.Generic;
using System.Linq;

namespace Engine.UI.Controls
{
    public class MultiSelectListBox : ListBoxBase
    {
        public event EngineEventHandler<MultiSelectListBox, List<ListBoxItem>> SelectedItemsChanged;

        private List<int> selectedIndices = new List<int>();

        public IReadOnlyList<int> SelectedIndices => selectedIndices;
        public IReadOnlyList<ListBoxItem> SelectedItems => items.Where(x => selectedIndices.Contains(x.Index)).ToList();

        public override void Clear() => selectedIndices.Clear();

        protected override ListBoxItemState ResolveItemState(int x)
        {
            ListBoxItemState s = CurrentState.Item;

            if (selectedIndices.Contains(x))
                s = CurrentState.ItemSelected;
            else if (x == hoverIndex)
                s = CurrentState.ItemHover;

            return s;
        }

        protected override void OnMouseClick(UiControlMouseEventArgs e)
        {
            bool changed = false;
            if (ui.Input.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                ui.Input.Keyboard.KeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift))
            {
                if (selectedIndices.Contains(hoverIndex))
                    selectedIndices.Remove(hoverIndex);
                else
                    selectedIndices.Add(hoverIndex);

                changed = true;
            }
            else
            {
                if (selectedIndices.Contains(hoverIndex))
                {
                    if (selectedIndices.Count == 0 || selectedIndices.Count > 1)
                    {
                        selectedIndices.Clear();
                        selectedIndices.Add(hoverIndex);
                        changed = true;
                    }
                    else
                    {
                        if (selectedIndices[0] != hoverIndex)
                        {
                            selectedIndices.Clear();
                            selectedIndices.Add(hoverIndex);
                            changed = true;
                        }
                    }
                }
                else
                {
                    selectedIndices.Clear();
                    selectedIndices.Add(hoverIndex);
                    changed = true;
                }
            }


            if (changed)
                SelectedItemsChanged?.Invoke(this, items.Where(x => selectedIndices.Contains(x.Index)).ToList());

            base.OnMouseClick(e);
        }
    }
}
