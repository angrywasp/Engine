using System.IO;
using System.Linq;
using System.Numerics;
using Engine.Content;
using Engine.Editor.Components;
using Engine.Editor.Components.Gizmo;
using Engine.Editor.Components.UI;
using Engine.Helpers;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class MapForm : EditorForm
    {
        public event EngineEventHandler<MapForm> Reload;

        private UiControl editorArea;
        private string path;
        private Map map;
        private (UiControl Control, GizmoControl<MapViewGizmoSelection, MapObject> Gizmo) objectControl;
        private static readonly Vector2i btnSize = new Vector2i(150, 25);
        private MapList lbx;

        public MapForm(EngineCore engine, Map map, string path) : base(engine)
        {
            this.map = map;
            this.path = path;
            ConstructLayout();
            engine.Scene.Physics.SimulationUpdate = false;
        }

        private void ConstructLayout()
        {
            int w = ui.Size.X;
            int h = 105;
            int y = ui.Size.Y - h;

            editorArea = this.Control(new Vector2i(0, 0), new Vector2i(w - 200, y), "editorArea", "BlankControl");

            var bottomControlPanel = this.Control(new Vector2i(0, y), new Vector2i(w, h), "bottomControlPanel", "UiControl");
            var btnObjects = bottomControlPanel.RadioButton(new Vector2i(10, 10), btnSize, "btnObjects", "Objects");
            var btnSave = bottomControlPanel.Button(new Vector2i(10, 70), btnSize, "btnSave", "Save");
            
            var btnReload = bottomControlPanel.Button(new Vector2i(170, 70), btnSize, "btnReload", "Reload");

            objectControl = CreateObjectLayout(bottomControlPanel, w, h);

            var sideControlPanel = this.Control(new Vector2i(w - 200, 0), new Vector2i(200, y), "sideControlPanel", "UiControl");
            lbx = new MapList(sideControlPanel, Vector2.Zero, Vector2.One, map);

            lbx.SelectedItemsChanged += (sender, items) =>
            {
                if (objectControl.Gizmo.Visible)
                    objectControl.Gizmo.SelectItems(items.Select(x => (MapObject)x.Tag));
            };

            btnObjects.CheckChanged += (s, isChecked) =>
            {
                if (!isChecked)
                    return;

                objectControl.Control.Visible = true;
                objectControl.Gizmo.Visible = true;
                lbx.UpdateDataStore();
            };

            btnSave.MouseClick += (s, e) => { Save(); };
            btnReload.MouseClick += (s, e) => { Reload?.Invoke(this); };

            btnObjects.Checked = true;
        }

        private (UiControl Control, GizmoControl<MapViewGizmoSelection, MapObject> Gizmo) CreateObjectLayout(UiControl parent, int w, int h)
        {
            var ctrl = parent.Control(new Vector2i(320, 0), new Vector2i(w - 320, h), "ObjectControl", "BlankControl");
            ctrl.Visible = false;

            var btnAdd = ctrl.Button(new Vector2i(10, 10), btnSize, "btnAdd", "Add");

            btnAdd.MouseClick += async (s, e) =>
            {
                var browserResult = await Browse(NodeType.Type).ConfigureAwait(false);
                if (!browserResult.ValidResult)
                    return;

                var name = GetFreeMapObjectName(Path.GetFileNameWithoutExtension(browserResult.Path));
                engine.Scene.CreateMapObject(name, browserResult.Path);
            };

            var gizmoControl = new GizmoControl<MapViewGizmoSelection, MapObject>(engine, parent, new Vector2i(parent.PixelSize.X - 500, 0), GizmoMode.UniformScale, "Component");

            gizmoControl.Translate += MapViewGizmoSelection.Translate;
            gizmoControl.Rotate += MapViewGizmoSelection.Rotate;
            gizmoControl.Scale += MapViewGizmoSelection.Scale;

            return (ctrl, gizmoControl);
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(map.PhysicsModelPath))
                ContentLoader.SaveJson(map.PhysicsModel, EngineFolders.ContentPathVirtualToReal(map.PhysicsModelPath));
                
            ContentLoader.SaveJson(map, EngineFolders.ContentPathVirtualToReal(path));
        }

        private string GetFreeMapObjectName(string prefix)
        {
            int start = 0;
            while (true)
            {
                string name = $"{prefix}_{start++}";
                if (map.Objects.Where(x => x.Name == name).FirstOrDefault() == null)
                    return name;
            }
        }

        public override bool ShouldUpdateCameraController() =>
            engine.Input.Mouse.InsideRect(editorArea.Bounds) &&
                objectControl.Gizmo.ActiveAxis == GizmoAxis.None;

        /*private T CreateComponent<T>(string name) where T : ComponentType, new()
        {
            if (gameObject.Type == null)
            {
                Log.Instance.WriteError("Cannot add components to the specified type");
                return null;
            }

            T ct = new T();
            ct.Name = name;

            return ct;
        }

        private void LoadComponent(ComponentType ct)
        {
            gameObject.Type.Components.Add(ct);
#pragma warning disable CS4014
            gameObject.LoadComponentAsync(ct);
#pragma warning restore CS4014
        }*/

        public override void ViewUpdate(GameTime gameTime)
        {  
            if (objectControl.Gizmo.Visible)
                objectControl.Gizmo.Update(gameTime);
        }

        public override void ViewDraw()
        {
            if (objectControl.Gizmo.Visible)
                objectControl.Gizmo.Draw();
        }
    }
}