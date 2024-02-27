using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Engine.AssetTransport;
using Engine.Editor.Components.UI;
using Engine.Graphics.Materials;
using Engine.Helpers;
using Engine.UI;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class MaterialForm : EditorForm
    {
        public delegate void ShapeChangedEventHandler(string shapePath);
        public event ShapeChangedEventHandler ShapeChanged;

        private string materialPath; 
        private MeshMaterial material;

        public MaterialForm(EngineCore engine, string materialPath) : base(engine)
        {
            this.materialPath = materialPath;
            ConstructLayout();
        }

        public void UpdateMeshMaterial(MeshMaterial material) => this.material = material;

        public override bool ShouldUpdateCameraController() => true;

        private void ConstructLayout()
        {
            var cbxShape = this.ComboBox(new Vector2(0.5f, 0f), new Vector2(0.1f, 0.05f));
            var btn2Sides = this.Button(new Vector2(0.4f, 0f), new Vector2(0.1f, 0.05f), text: "2 Sides");

            var cbxMatChannel = this.ComboBox(new Vector2(0.6f, 0), new Vector2(0.2f, 0.05f));
            var lbxTextures = new FileList(this, new Vector2(0.8f, 0f), new Vector2(0.2f, 0.95f), "*.texture");
            var btnApply = this.Button(new Vector2(0.8f, 0.95f), new Vector2(0.1f, 0.05f), text: "Set");
            var btnSave = this.Button(new Vector2(0.9f, 0.95f), new Vector2(0.1f, 0.05f), text: "Save");

            btn2Sides.MouseClick += (s, e) =>
            {
                material.DoubleSided = !material.DoubleSided;
            };

            var shapes = new Dictionary<string, object>();
            shapes.Add("Sphere", "Engine/Renderer/Meshes/Sphere.mesh");
            shapes.Add("Cube", "Engine/Renderer/Meshes/Cube.mesh");
            shapes.Add("Cylinder", "Engine/Renderer/Meshes/Cylinder.mesh");
            
            var ds = new Dictionary<string, object>();
            ds.Add($"Albedo", 0);
            ds.Add($"Normal", 1);
            ds.Add($"PBR", 2);
            ds.Add($"Emissive", 3);
            
            cbxShape.DataBind(shapes);
            cbxShape.SelectedIndex = 0;
            cbxMatChannel.DataBind(ds);
            cbxMatChannel.SelectedIndex = 0;

            btnApply.MouseClick += (s, e) =>
            {
                if (cbxMatChannel.SelectedItem == null)
                    return;

                if (lbxTextures.SelectedItem == null)
                    return;

                int index = (int)cbxMatChannel.SelectedItem.Tag;
                string texPath = lbxTextures.SelectedItem.Tag.ToString();

                switch (index)
                {
                    case 0:
                        material.AlbedoMap = texPath;
                        break;
                    case 1:
                        material.NormalMap = texPath;
                        break;
                    case 2:
                        material.PbrMap = texPath;
                        break;
                    case 3:
                        material.EmissiveMap = texPath;
                        break;
                }
            };

            cbxShape.SelectedItemChanged += (s, e) => {
                ShapeChanged?.Invoke(e.Item.Tag.ToString());
            };

            btnSave.MouseClick += (s, e) => {
                File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(materialPath), MeshMaterialWriter.Write(material));
            };
        }

        public override void ViewUpdate(GameTime gameTime) { }

        public override void ViewDraw() { }
    }
}