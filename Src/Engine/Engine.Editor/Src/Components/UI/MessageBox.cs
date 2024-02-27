using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Engine.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Components.UI
{
    public class MessageBox
    {
        private Interface ui;
        private string title;
        private string message;

        private UiControl overlay;
        private Button btnOk;
        private Button btnCancel;

        private UiForm previousForm;
        private UiForm form;

        private EventWaitHandle waitHandle;

        private Vector2i sz = new Vector2i(512, 128);

        public MessageBox(Interface ui, string title, string message)
        {
            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.ui = ui;
            this.title = title;
            this.message = message;

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

            var txtMessage = root.TextBox(new Vector2i(0, 32), new Vector2i(sz.X, 64));
            txtMessage.Text = message;
            txtMessage.TextHorizontalAlignment = TextBox.Align_Mode.Center;
            txtMessage.TextVerticalAlignment = TextBox.Align_Mode.Center;

            btnCancel = root.Button(new Vector2i(0, sz.Y - 32), new Vector2i(sz.X / 2, 32), text: "Cancel");
            btnOk = root.Button(new Vector2i(sz.X / 2, sz.Y - 32), new Vector2i(sz.X / 2, 32), text: "OK");
        }

        public async Task<bool> Show()
        {
            bool result = false;

            btnCancel.MouseClick += (s, e) => {
                result = false;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            btnOk.MouseClick += (s, e) => {
                result = true;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            ui.SetMainForm(form);

            await Task.Run(() => waitHandle.WaitOne());
            return result;
        }
    }
}