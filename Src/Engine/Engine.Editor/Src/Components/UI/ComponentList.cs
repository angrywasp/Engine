using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.UI
{
    public class ComponentList
    {
        public event EngineEventHandler<ComponentList, List<ListBoxItem>> SelectedItemsChanged;

        private UiControl parent;
        private List<ListBoxItem> selectedItems;
        private GameObject gameObject;
        private MultiSelectListBox lbx;
        private bool listPhysics = false;
        private bool physicsModelEventHandlersAdded = false;

        public IReadOnlyList<ListBoxItem> SelectedItems => selectedItems;

        public ComponentList(UiControl parent, Vector2 position, Vector2 size, GameObject gameObject)
        {
            this.parent = parent;
            this.gameObject = gameObject;
            ConstructLayout(parent, position, size);
        }

        public ComponentList(UiControl parent, Vector2i position, Vector2i size, GameObject gameObject)
        {
            this.parent = parent;
            this.gameObject = gameObject;
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

            this.gameObject.ComponentAddedToMap += (k, v) => { UpdateDataStore(); };
            this.gameObject.ComponentRemovedFromMap += (k, v) => { UpdateDataStore(); };

            AddPhysicsModelEventHandlers();
        }

        public void ConstructLayout(UiControl parent, Vector2i position, Vector2i size)
        {
            var sc = parent.ScrollableContainer(position, size);
            lbx = sc.ContentArea.MultiSelectListBox(Vector2.Zero, Vector2.One);

            lbx.SelectedItemsChanged += (s, e) => {
                this.selectedItems = e;
                SelectedItemsChanged?.Invoke(this, e);
            };

            this.gameObject.ComponentAddedToMap += (k, v) => { UpdateDataStore(); };
            this.gameObject.ComponentRemovedFromMap += (k, v) => { UpdateDataStore(); };

            AddPhysicsModelEventHandlers();
        }

        private void AddPhysicsModelEventHandlers()
        {
            if (gameObject.PhysicsModel == null || physicsModelEventHandlersAdded)
                return;

            this.gameObject.PhysicsModel.ShapeAdded += (s, e) => { UpdateDataStore(); };
            this.gameObject.PhysicsModel.ShapeRemoved += (s, e) => { UpdateDataStore(); };

            physicsModelEventHandlersAdded = true;
        }

        public void UpdateDataStore()
        {
            if (listPhysics)
            {
                if (gameObject.PhysicsModel != null)
                    lbx.DataBind(gameObject.PhysicsModel.Shapes.ToDictionary(x => $"{x.ToString()}_{gameObject.PhysicsModel.Shapes.IndexOf(x).ToString()}", x => x));
                else
                    lbx.DataBind(new Dictionary<string, object>());
            }
            else
                lbx.DataBind(gameObject.Components.ToDictionary(x => x.Key, x => x.Value));
        }

        public void ListComponents()
        {
            listPhysics = false;
            UpdateDataStore();
        }

        public void ListPhysics()
        {
            listPhysics = true;
            AddPhysicsModelEventHandlers();
            UpdateDataStore();
        }
    }
}