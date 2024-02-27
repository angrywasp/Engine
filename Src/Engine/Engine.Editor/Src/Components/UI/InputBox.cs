using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.UI
{
    public class InputBox
    {
        private Interface ui;
        private string title;

        private UiControl overlay;
        private Button btnOk;
        private Button btnCancel;
        private TextInput txtInput;

        private UiForm previousForm;
        private UiForm form;

        private EventWaitHandle waitHandle;

        private Vector2i sz = new Vector2i(512, 128);

        public InputBox(Interface ui, string title)
        {
            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.ui = ui;
            this.title = title;

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
            txtTile.Text = title;

            txtInput = root.TextInput(new Vector2i(32, 48), new Vector2i(sz.X - 64, 32));

            int x = sz.X / 4;

            btnCancel = root.Button(new Vector2i(x * 2, sz.Y - 32), new Vector2i(sz.X / 4, 32), text: "Cancel");
            btnOk = root.Button(new Vector2i(x * 3, sz.Y - 32), new Vector2i(sz.X / 4, 32), text: "OK");
        }

        public async Task<string> Show()
        {
            string result = null;

            btnCancel.MouseClick += (s, e) => {
                result = null;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            btnOk.MouseClick += (s, e) => {
                if (string.IsNullOrEmpty(txtInput.Text))
                    return;
                    
                result = txtInput.Text;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            ui.SetMainForm(form);

            await Task.Run(() => waitHandle.WaitOne());
            return result;
        }
    }
}