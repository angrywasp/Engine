using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using Engine.Editor.InstanceEditor;
using Engine.Editor.InstanceEditor.Tools;
using Engine.UI;
using Engine.Editor.Components.UI;
using Engine.Editor.Scripts;
using Engine.World.Objects;
using Engine.World;

namespace Engine.Editor.Forms
{
    public class InstanceManagerForm : EditorForm
    {
        private ToolManager toolManager;
        private InstanceTool activeTool;
        private HeightmapTerrain terrain;

        private UiControl root;
        
        public InstanceManagerForm(EngineCore engine, Map map, HeightmapTerrain terrain) : base(engine)
        {
            this.terrain = terrain;
            toolManager = new ToolManager(engine, map, terrain);
            ConstructLayout();
        }

        private void ConstructLayout()
        {
            Vector2i btnSize = new Vector2i(150, 25);

            root = this.Control(new Vector2i(0, ui.Size.Y - 105), new Vector2i(ui.Size.X, 105), skinElement: "UiControl");

            var btnQuit = root.Button(new Vector2i(10, 10), btnSize, text: "Quit");
            var btnPaint = root.RadioButton(new Vector2i(10, 40), btnSize, text: "Paint");
            var btnPlace = root.RadioButton(new Vector2i(10, 70), btnSize, text: "Place");

            var commonRoot = root.Control(new Vector2i(170, 0), new Vector2i(480, 100), skinElement: "BlankControl");
            var paintRoot = root.Control(new Vector2i(660, 0), new Vector2i(410, 100), skinElement: "BlankControl");

            { //main form
                var btnSingle = commonRoot.TextBox(new Vector2i(0, 10), btnSize, text: "Single");
                var btnRandom = commonRoot.TextBox(new Vector2i(160, 10), btnSize, text: "Random");
                var btnBrowse = commonRoot.Button(new Vector2i(320, 10), btnSize, text: "Browse");
                var txtSelection = commonRoot.TextBox(new Vector2i(0, 40), new Vector2i(470, 25), text: "Select mesh");
                var btnAlignToTerrain = commonRoot.CheckBox(new Vector2i(0, 70), btnSize, text: "Align");

                var activeArea = ui.Size - new Vector2i(0, 100);

                btnQuit.MouseClick += (s, e) =>
                {
                    activeTool = null;
                    engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Place.ToolShape);
                    engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Paint.ToolShape);

                    foreach (var ctrl in root)
                    {
                        if (!(ctrl is RadioButton))
                            continue;

                        ((RadioButton)ctrl).Checked = false;
                    }
                };

                btnPaint.MouseClick += (s, e) =>
                {
                    paintRoot.Visible = true;

                    activeTool = toolManager.Paint;
                    engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Place.ToolShape);
                    engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(toolManager.Paint.ToolShape);
                };

                btnPlace.MouseClick += (s, e) =>
                {
                    paintRoot.Visible = false;
                    
                    activeTool = toolManager.Place;
                    engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Paint.ToolShape);
                    engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(toolManager.Place.ToolShape);
                };

                btnBrowse.MouseClick += async (s, e) =>
                {
                    var fileBrowserResult = await new FileBrowser(engine.Interface).Show().ConfigureAwait(false);

                    if (fileBrowserResult.Type == FileBrowser.File_Type.Invalid)
                        return;

                    MapScripts.SetRandomMeshFolder(fileBrowserResult.Path);
                    txtSelection.Text = fileBrowserResult.Path;

                    switch (fileBrowserResult.Type)
                    {
                        case FileBrowser.File_Type.Directory:
                            btnRandom.ApplySkinElementState("Active");
                            btnSingle.ApplySkinElementState("Normal");
                            break;
                        case FileBrowser.File_Type.File:
                            btnSingle.ApplySkinElementState("Active");
                            btnRandom.ApplySkinElementState("Normal");
                            break;
                    }
                };

                btnAlignToTerrain.CheckChanged += (s, e) => {
                    toolManager.Paint.AlignToTerrain = e;
                    toolManager.Place.AlignToTerrain = e;
                };
            }

            { //paint controls. radius and density
                var slRadius = paintRoot.Slider(new Vector2i(0, 10), new Vector2i(200, 25), value: PaintTool.DEFAULT_RADIUS);
                var slStrength = paintRoot.Slider(new Vector2i(0, 40), new Vector2i(200, 25), value: PaintTool.DEFAULT_STRENGTH);
                var txtRadius = paintRoot.TextBox(new Vector2i(210, 10), new Vector2i(120, 25), text: $"Rad: {PaintTool.DEFAULT_RADIUS}");
                var txtStrength = paintRoot.TextBox(new Vector2i(210, 40), new Vector2i(120, 25), text: $"Str: {PaintTool.DEFAULT_STRENGTH}");

                slRadius.ValueChanged += (s, e) =>
                {
                    txtRadius.Text = $"Rad: {(int)e.NewValue}";
                    if (activeTool is PaintTool)
                        ((PaintTool)activeTool).Radius = slRadius.Value;
                };

                slStrength.ValueChanged += (s, e) =>
                {
                    txtStrength.Text = $"Str: {(int)e.NewValue}";
                    if (activeTool is PaintTool)
                        ((PaintTool)activeTool).Density = slStrength.Value;
                };
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            activeTool?.Update(gameTime);
        }

        public override bool ShouldUpdateCameraController()
        {
            if (engine.Input.Mouse.InsideRect(root.Bounds))
                return false;

            if (activeTool != null && activeTool.IsActive)
                return false;

            return true;
        }

        public override void ViewUpdate(GameTime gameTime) { }

        public override void ViewDraw() { }
    }
}