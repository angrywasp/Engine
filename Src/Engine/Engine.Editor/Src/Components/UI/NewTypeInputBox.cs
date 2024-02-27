using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Content;
using Engine.Graphics.Materials;
using Engine.Helpers;
using Engine.UI;
using Engine.UI.Controls;
using Engine.World;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.Editor.Components.UI
{
    public class NewTypeInputBox
    {
        private Interface ui;
        private UiControl overlay;
        private Button btnOk;
        private Button btnCancel;
        private TextInput txtInput;
        private ComboBox cbxTypes;

        private UiForm previousForm;
        private UiForm form;

        private EventWaitHandle waitHandle;

        private Vector2i sz = new Vector2i(512, 128);

        private string virtualPath;

        public NewTypeInputBox(Interface ui, string virtualPath)
        {
            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.ui = ui;
            this.virtualPath = virtualPath;
            this.previousForm = ui.MainForm;

            form = new UiForm(ui);
            form.ApplySkinElement("BlankControl");
            form.Control(Vector2.Zero, Vector2.One, skinElement: "BlankControl");
            form.Load();

            overlay = form.Add<UiControl>(Vector2.Zero, Vector2.One);
            overlay.Load();

            var pos = (ui.Size - sz) / 2;

            var root = overlay.Add<UiControl>(pos, sz);
            root.Load();

            ConstructLayout(root);
        }

        private void ConstructLayout(UiControl root)
        {
            var txtTile = root.TextBox(new Vector2i(0, 0), new Vector2i(sz.X, 32));
            txtTile.Text = "New Asset";

            txtInput = root.TextInput(new Vector2i(32, 48), new Vector2i(sz.X - 64, 32));

            int x = sz.X / 4;

            btnCancel = root.Button(new Vector2i(x * 2, sz.Y - 32), new Vector2i(sz.X / 4, 32), text: "Cancel");
            btnOk = root.Button(new Vector2i(x * 3, sz.Y - 32), new Vector2i(sz.X / 4, 32), text: "OK");

            cbxTypes = root.ComboBox(new Vector2i(0, sz.Y - 32), new Vector2i(sz.X / 2, 32));

            var ds = new Dictionary<string, object>();
            ds.Add("Physics", NodeType.Physics);
            ds.Add("Type", NodeType.Type);
            ds.Add("Map", NodeType.Map);
            cbxTypes.DataBind(ds);
        }

        public async Task<bool> Show()
        {
            bool result = false;

            btnCancel.MouseClick += (s, e) => {
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            btnOk.MouseClick += (s, e) => {
                if (string.IsNullOrEmpty(txtInput.Text))
                    return;

                if (cbxTypes.SelectedItem == null)
                    return;

                result = CreateAsset();
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            ui.SetMainForm(form);

            await Task.Run(() => waitHandle.WaitOne());
            return result;
        }

        private bool CreateAsset()
        {
            var nodeType = (NodeType)cbxTypes.SelectedItem.Tag;
            var fileName = $"{txtInput.Text}.{nodeType.ToString().ToLower()}";

            var fullNewPath = Path.Combine(EngineFolders.ContentPathVirtualToReal(virtualPath), fileName).NormalizeFilePath();

            switch (nodeType)
            {
                case NodeType.Physics:
                    {
                        var asset = new Physics.PhysicsModel();
                        string jsonString = JsonConvert.SerializeObject(asset, ContentLoader.DefaultJsonSerializerOptions());
                        File.WriteAllText(fullNewPath, jsonString);
                    }
                    break;
                case NodeType.Type:
                    {
                        var asset = new GameObjectType();
                        string jsonString = JsonConvert.SerializeObject(asset, ContentLoader.DefaultJsonSerializerOptions());
                        File.WriteAllText(fullNewPath, jsonString);
                        EngineCore.Instance.TypeFactory.CacheTypeFile(fullNewPath);
                    }
                    break;
                case NodeType.Map:
                    {
                        var asset = new Map();
                        string jsonString = JsonConvert.SerializeObject(asset, ContentLoader.DefaultJsonSerializerOptions());
                        File.WriteAllText(fullNewPath, jsonString);
                    }
                    break;
                default:
                    Log.Instance.WriteError($"NodeType {nodeType} is not supported for new asset creation");
                    return false;
            }

            return true;
        }
    }
}