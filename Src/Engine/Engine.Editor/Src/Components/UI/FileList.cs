using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Engine.Helpers;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.UI
{
    public class FileList
    {
        public event EngineEventHandler<FileList, (int Index, ListBoxItem Item)> SelectedItemChanged;

        private UiControl parent;
        private string searchPattern;
        private ScrollableContainer scrollableContainer;

        private ListBoxItem selectedItem;

        public ListBoxItem SelectedItem => selectedItem;

        public Rectangle Bounds => scrollableContainer.Bounds;

        public FileList(UiControl parent, Vector2 position, Vector2 size, string searchPattern)
        {
            this.parent = parent;
            this.searchPattern = searchPattern;
            ConstructLayout(parent, position, size);
        }

        public void ConstructLayout(UiControl parent, Vector2 position, Vector2 size)
        {
            scrollableContainer = parent.ScrollableContainer(position, size);
            var lbx = scrollableContainer.ContentArea.ListBox(Vector2.Zero, Vector2.One);

            var ds = new SortedDictionary<string, string>();
            var files = Directory.GetFiles(EngineFolders.ContentPath, searchPattern, SearchOption.AllDirectories).Where(x => !Path.GetFileName(x).StartsWith("."));

            foreach (var f in files)
                ds.Add(Path.GetFileNameWithoutExtension(f), EngineFolders.ContentPathRealToVirtual(f));

            lbx.SelectedItemChanged += (sender, i) => {
                this.selectedItem = i.Item;
                SelectedItemChanged?.Invoke(this, i);
            };

            lbx.DataBind(ds);
        }
    }
}