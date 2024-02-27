using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Editor.TerrainEditor.Tools;
using Engine.Editor.TerrainEditor;
using Engine.Editor.TerrainEditor.Brushes;
using System;
using Engine.World.Objects;

namespace Engine.Editor.Forms
{
    public class HeightmapTerrainForm : EditorForm
    {
        private BrushManager brushManager;
        private ToolManager toolManager;
        private TerrainTool activeTool;
        private HeightmapTerrain terrain;

        private UiControl root;

        private Slider slRadius;
        private Slider slStrength;

        public HeightmapTerrainForm(EngineCore engine, HeightmapTerrain terrain) : base(engine)
        {
            this.terrain = terrain;
            brushManager = new BrushManager(engine);
            toolManager = new ToolManager(engine, terrain);
            brushManager.LoadAsync().GetAwaiter().GetResult();
            ConstructLayout();
        }

        private void ConstructLayout()
        {
            Vector2i btnSize = new Vector2i(150, 25);

            root = this.Control(new Vector2i(0, ui.Size.Y - 105), new Vector2i(ui.Size.X, 105), skinElement: "UiControl");

            var btnQuit = root.Button(new Vector2i(10, 10), btnSize, text: "Quit");
            var btnPaint = root.RadioButton(new Vector2i(10, 40), btnSize, text: "Paint");
            var btnErase = root.RadioButton(new Vector2i(10, 70), btnSize, text: "Erase");

            var paintRoot = root.Control(new Vector2i(170, 0), new Vector2i(410, 100), skinElement: "BlankControl");
            var brushRoot = root.Control(new Vector2i(580, 0), new Vector2i(200, 100), skinElement: "BlankControl");

            ConstructPaintTools(paintRoot);
            ConstructBrushTools(brushRoot);

            btnQuit.MouseClick += delegate (UiControl sender, UiControlMouseEventArgs e)
            {
                activeTool = null;
                engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Erase.ToolShape);
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
                paintRoot.Visible = brushRoot.Visible = true;
                activeTool = toolManager.Paint;
                engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Erase.ToolShape);
                engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(toolManager.Paint.ToolShape);
            };

            btnErase.MouseClick += (s, e) =>
            {
                paintRoot.Visible = false;
                activeTool = toolManager.Erase;
                engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(toolManager.Paint.ToolShape);
                engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(toolManager.Erase.ToolShape);
            };
        }

        private void ConstructPaintTools(UiControl ctrl)
        {
            var sz = Vector2i.One * 80;
            var x = 10;
            var y = (ctrl.PixelSize.Y - sz.Y) / 2;
            var space = 20;

            var btnLayer0 = ctrl.RadioButton(new Vector2i(x, y), sz);
            AddBrushSkin(btnLayer0, texturePath: terrain.Material.DiffuseMap0Path);
            btnLayer0.MouseClick += (s, e) => { toolManager.Paint.Index = 0; };

            x += (sz.X + space);

            var btnLayer1 = ctrl.RadioButton(new Vector2i(x, y), sz);
            AddBrushSkin(btnLayer1, texturePath: terrain.Material.DiffuseMap1Path);
            btnLayer1.MouseClick += (s, e) => { toolManager.Paint.Index = 1; };

            x += (sz.X + space);

            var btnLayer2 = ctrl.RadioButton(new Vector2i(x, y), sz);
            AddBrushSkin(btnLayer2, texturePath: terrain.Material.DiffuseMap2Path);
            btnLayer2.MouseClick += (s, e) => { toolManager.Paint.Index = 2; };

            x += (sz.X + space);

            var btnLayer3 = ctrl.RadioButton(new Vector2i(x, y), sz);
            AddBrushSkin(btnLayer3, texturePath: terrain.Material.DiffuseMap3Path);
            btnLayer3.MouseClick += (s, e) => { toolManager.Paint.Index = 3; };

            ctrl.Visible = false;
        }

        private void ConstructBrushTools(UiControl ctrl)
        {
            var sz = Vector2i.One * 40;
            var x = 10;
            var y = 10;
            var space = 10;
            
            var btnCircle = ctrl.RadioButton(new Vector2i(x, y), sz, "btnCircle");
            AddBrushSkin(btnCircle, texture: brushManager.Circle.Mask);
            btnCircle.MouseClick += btnBrush_MouseClick;

            var btnSquare = ctrl.RadioButton(new Vector2i(sz.X + (x * 2), y), sz, "btnSquare");
            AddBrushSkin(btnSquare, texture: brushManager.Square.Mask);
            btnSquare.MouseClick += btnBrush_MouseClick;

            var btnRadGrad = ctrl.RadioButton(new Vector2i(x, sz.Y + (y * 2)), sz, "btnRadGrad");
            AddBrushSkin(btnRadGrad, texture: brushManager.RadialGradient.Mask);
            btnRadGrad.MouseClick += btnBrush_MouseClick;

            x = (sz.X * 2) + (x * 4);
            sz = new Vector2i(200, 25);

            slRadius = ctrl.Slider(new Vector2i(x, 10), sz, null, 1, 100, BrushManager.DEFAULT_RADIUS);
            TextBox txtRadius = ctrl.TextBox(new Vector2i(sz.X + x + space, 10), new Vector2i(120, 25));
            txtRadius.Text = "Rad: " + BrushManager.DEFAULT_RADIUS.ToString();

            slStrength = ctrl.Slider(new Vector2i(x, 40), sz, null, 0, 255, TerrainTool.DEFAULT_STRENGTH);
            TextBox txtStrength = ctrl.TextBox(new Vector2i(sz.X + x + space, 40), new Vector2i(120, 25));
            txtStrength.Text = "Str: " + TerrainTool.DEFAULT_STRENGTH.ToString();

            slRadius.ValueChanged += delegate (UiControl sender, SliderValueChangedEventArgs e)
            {
                int intVal = (int)e.NewValue;
                txtRadius.Text = "Rad: " + intVal.ToString();

                if (activeTool != null && activeTool.Brush != null)
                    ((TerrainTool)activeTool).Brush.Radius = intVal;
            };

            slStrength.ValueChanged += delegate (UiControl sender, SliderValueChangedEventArgs e)
            {
                int intVal = (int)e.NewValue;
                txtStrength.Text = "Str: " + intVal.ToString();

                if (activeTool != null)
                    ((TerrainTool)activeTool).Strength = intVal;
            };

            ctrl.Visible = false;
        }

        void btnBrush_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            int radius = (int)slRadius.Value;
            int strength = (int)slStrength.Value;
            toolManager.Paint.Strength = strength;

            TerrainBrush terrainBrush = null;

            switch (sender.Name)
            {
                case "btnCircle":
                    terrainBrush = brushManager.Circle;
                    break;
                case "btnSquare":
                    terrainBrush = brushManager.Square;
                    break;
                case "btnRadGrad":
                    terrainBrush = brushManager.RadialGradient;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (terrainBrush != null)
            {
                toolManager.Paint.Brush = terrainBrush;
                toolManager.Erase.Brush = terrainBrush;

                toolManager.Paint.Brush.Radius = radius;
                toolManager.Erase.Brush.Radius = radius;
            }
        }

        private void AddBrushSkin(RadioButton btn, string texturePath = null, Texture2D texture = null)
        {
            if (!ui.Skin.Elements.ContainsKey(btn.Name))
            {
                var skinElement = new SkinElement();

                var n = new ControlState();
                var o = new ControlState();
                var a = new ControlState();
                var d = new ControlState();

                var cn = new ControlState();
                var co = new ControlState();
                var ca = new ControlState();
                var cd = new ControlState();

                n.Background.Color = Color.White;

                cn.Background.Color = Color.Green;

                o.Background.Color = Color.Green;
                co.Background.Color = Color.LimeGreen;

                a.Background.Color = Color.DarkRed;
                ca.Background.Color = Color.Red;

                d.Background.Color = Color.DimGray;
                cd.Background.Color = Color.DimGray;

                skinElement.States.Add("Normal", n);
                skinElement.States.Add("Over", o);
                skinElement.States.Add("Active", a);
                skinElement.States.Add("Disabled", d);
                skinElement.States.Add("CheckedNormal", cn);
                skinElement.States.Add("CheckedOver", co);
                skinElement.States.Add("CheckedActive", ca);
                skinElement.States.Add("CheckedDisabled", cd);

                foreach (var state in skinElement.States.Values)
                {
                    if (texture != null)
                    {
                        state.Background.Texture = texture;
                        state.Background.Region = new Rectangle(0, 0, texture.Width, texture.Height);
                    }
                    else if (!string.IsNullOrEmpty(texturePath))
                        state.Background.TexturePath = texturePath;
                }

                EngineCore.Instance.Interface.Skin.AddElement(skinElement, btn.Name);
                skinElement.Load(EngineCore.Instance.Interface);
            }

            btn.Text = string.Empty;
            btn.SkinElementKey = btn.Name;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            activeTool?.Update(gameTime);
        }

        public override void Draw()
        {
            base.Draw();
            activeTool?.Draw();
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