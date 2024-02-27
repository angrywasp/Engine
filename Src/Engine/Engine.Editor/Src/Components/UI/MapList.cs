using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.UI
{
    public class MapList
    {
        public event EngineEventHandler<MapList, List<ListBoxItem>> SelectedItemsChanged;

        private UiControl parent;
        private List<ListBoxItem> selectedItems;
        private Map map;
        private MultiSelectListBox lbx;

        public IReadOnlyList<ListBoxItem> SelectedItems => selectedItems;

        public MapList(UiControl parent, Vector2 position, Vector2 size, Map map)
        {
            this.parent = parent;
            this.map = map;
            ConstructLayout(parent, position, size);
        }

        public MapList(UiControl parent, Vector2i position, Vector2i size, Map map)
        {
            this.parent = parent;
            this.map = map;
            ConstructLayout(parent, position, size);
        }

        public void ConstructLayout(UiControl parent, Vector2 position, Vector2 size)
        {
            var sc = parent.ScrollableContainer(position, size);
            lbx = sc.ContentArea.MultiSelectListBox(Vector2.Zero, Vector2.One);

            lbx.SelectedItemsChanged += (s, e) => {
                this.selectedItems = e;
                SelectedItemsChanged?.Invoke(this, e);
            };

            this.map.ObjectAddedToMap += (k, v) => { UpdateDataStore(); };
            this.map.ObjectRemovedFromMap += (k, v) => { UpdateDataStore(); };
            UpdateDataStore();
        }

        public void ConstructLayout(UiControl parent, Vector2i position, Vector2i size)
        {
            var sc = parent.ScrollableContainer(position, size);
            lbx = sc.ContentArea.MultiSelectListBox(Vector2.Zero, Vector2.One);

            lbx.SelectedItemsChanged += (s, e) => {
                this.selectedItems = e;
                SelectedItemsChanged?.Invoke(this, e);
            };

            this.map.ObjectAddedToMap += (k, v) => { UpdateDataStore(); };
            this.map.ObjectRemovedFromMap += (k, v) => { UpdateDataStore(); };
            UpdateDataStore();
        }

        public void UpdateDataStore()
        {
            lbx.DataBind(map.Objects.ToDictionary(x => x.Name, x => x));
        }
    }
}