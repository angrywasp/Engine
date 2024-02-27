using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Engine.AssetTransport;
using Engine.Content.Model;
using Engine.Content.Model.Instance;
using Engine.Editor.Components;
using Engine.Editor.Components.Gizmo;
using Engine.Editor.Components.UI;
using Engine.Helpers;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Engine.World.Components;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class MeshForm : EditorForm
    {
        private MeshInstance mesh;
        private string meshPath;
        private UiControl editorArea;
        private GizmoControl<MapViewGizmoSelection, MapObject> gizmoControl;
        private RuntimeMapObject container;
        private static readonly Vector2i btnSize = new Vector2i(150, 25);

        public MeshForm(EngineCore engine, string meshPath, MeshInstance mesh, RuntimeMapObject container) : base(engine)
        {
            this.meshPath = meshPath;
            this.mesh = mesh;
            this.container = container;
            ConstructLayout();
        }

        public override bool ShouldUpdateCameraController() => engine.Input.Mouse.InsideRect(editorArea.Bounds) && gizmoControl?.ActiveAxis == GizmoAxis.None;

        private void ConstructLayout()
        {
            int w = ui.Size.X;
            int h = 105;
            int y = ui.Size.Y - h;

            editorArea = this.Control(new Vector2i(0, 0), new Vector2i(w, y), "editorArea", "BlankControl");

            var bottomControlPanel = this.Control(new Vector2i(0, y), new Vector2i(w, h), "bottomControlPanel", "UiControl");
            var btnSave = bottomControlPanel.Button(new Vector2i(10, 70), btnSize, "btnSave", "Save");

            var cbxSubMesh = bottomControlPanel.ComboBox(new Vector2i(170, 10), new Vector2i(250, 25), "cbxSubMesh");
            var index = 0;
            cbxSubMesh.DataBind(mesh.SubMeshes.ToDictionary(k => $"SubMesh {index++}", v => v));
            cbxSubMesh.DropDirection = ComboBox_DropDirection.Up;

            var btnBrowseMaterial = bottomControlPanel.Button(new Vector2i(430, 10), btnSize, "btnBrowseMaterial", "Material");

            if (mesh.Skeleton != null)
            {
                var cbxAnimations = bottomControlPanel.ComboBox(new Vector2i(170, 40), new Vector2i(250, 25), "cbxAnimations");
                cbxAnimations.SelectedItemChanged += (s, e) => { mesh.UpdateSelectedAnimation(e.Item.Text); };
                cbxAnimations.DataBind(mesh.Skeleton.AnimationTracks.ToDictionary(k => k.Name, v => v));
            }
            else
            {
                gizmoControl = new GizmoControl<MapViewGizmoSelection, MapObject>(engine, bottomControlPanel, new Vector2i(bottomControlPanel.PixelSize.X - 500, 0), GizmoMode.UniformScale, "gizmoControl");

                gizmoControl.Translate += MapViewGizmoSelection.Translate;
                gizmoControl.Rotate += MapViewGizmoSelection.Rotate;
                gizmoControl.Scale += MapViewGizmoSelection.Scale;

                gizmoControl.SelectItems(new List<MapObject> { container });
            }

            btnBrowseMaterial.MouseClick += async (s, e) =>
            {
                if (cbxSubMesh.SelectedItem == null)
                    return;

                var browserResult = await Browse(NodeType.Material).ConfigureAwait(false);
                if (!browserResult.ValidResult)
                    return;

                await mesh.SetMaterialAsync(engine, browserResult.Path, cbxSubMesh.SelectedIndex).ConfigureAwait(false);
            };

            btnSave.MouseClick += (s, e) =>
            {
                var transformed = MeshUtils.Transform(((MeshComponent)container.GameObject.Components["Container"]).Mesh.Template, container.Transform.Matrix);
                File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(meshPath), MeshWriter.Write(transformed));
                ui.ScreenMessages.Write($"Saved '{meshPath}'", Color.DarkCyan);
                container.InitialTransform = new WorldTransform3();
                container.Transform.Update(Matrix4x4.Identity);
            };
        }

        public override void ViewUpdate(GameTime gameTime) => gizmoControl?.Update(gameTime);

        public override void ViewDraw() => gizmoControl?.Draw();
    }
}