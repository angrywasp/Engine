using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Engine.Helpers;
using Engine.UI;
using Engine.UI.Controls;

namespace Engine.Editor.Components.UI
{
    public class FileBrowser
    {
        public enum File_Type
        {
            Invalid,
            Directory,
            File
        }

        private Button btnBack;
        private TextBox txtPath;
        private Button btnClose;
        private Button btnOpen;
        private Button btnNewFolder;
        private Button btnNewFile;
        private ScrollableContainer sc;
        private ListBox lbx;

        private UiControl overlay;
        private UiForm previousForm;
        private UiForm form;

        private Interface ui;
        private EventWaitHandle waitHandle;
        private string searchPattern;

        private static (File_Type Type, string Path) current = default;

        public FileBrowser(Interface ui, string searchPattern = "*")
        {
            this.waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.searchPattern = searchPattern;
            this.ui = ui;
            this.previousForm = ui.MainForm;

            form = new UiForm(ui);
            form.ApplySkinElement("BlankControl");
            form.Control(Vector2.Zero, Vector2.One, skinElement: "BlankControl");
            form.Load();

            overlay = form.Add<UiControl>(Vector2.Zero, Vector2.One);
            overlay.Load();

            var root = overlay.Add<UiControl>(new Vector2(0.25f, 0.25f), new Vector2(0.5f, 0.5f));
            root.Load();

            ConstructLayout(root);

            switch (current.Type)
            {
                case File_Type.File:
                    current = (File_Type.Directory, Path.GetDirectoryName(current.Path));
                    break;
                case File_Type.Invalid:
                    current = (File_Type.Directory, string.Empty);
                    break;
            }

            lbx.DataBind(SetCurrentDirectory());
        }

        private void ConstructLayout(UiControl root)
        {
            btnBack = root.Button(Vector2.Zero, new Vector2(0.05f, 0.05f), text: "^");
            txtPath = root.TextBox(new Vector2(0.05f, 0), new Vector2(0.9f, 0.05f));

            btnClose = root.Button(new Vector2(0.95f, 0), new Vector2(0.05f, 0.05f), text: "X");

            btnNewFolder = root.Button(new Vector2(0, 0.95f), new Vector2(0.25f, 0.05f), text: "New Folder");
            btnNewFile = root.Button(new Vector2(0.25f, 0.95f), new Vector2(0.25f, 0.05f), text: "New File");
            btnOpen = root.Button(new Vector2(0.5f, 0.95f), new Vector2(0.5f, 0.05f), text: "Open");

            sc = root.ScrollableContainer(new Vector2(0, 0.05f), new Vector2(1, 0.9f));
            lbx = sc.ContentArea.ListBox(Vector2.Zero, Vector2.One);
            lbx.VerticalSizeMode = ListBox.Vertical_Size_Mode.FitToItems;

            btnBack.MouseClick += (s, e) => {
                if (current.Type == File_Type.Invalid)
                    return;

                if (current.Type == File_Type.File)
                    current = (File_Type.Directory, Path.GetDirectoryName(current.Path));

                current.Path = FileHelper.MoveUpDirectories(current.Path, 1);
                lbx.DataBind(SetCurrentDirectory());
            };

            lbx.SelectedItemChanged += (s, i) => {
                current = ((File_Type, string))i.Item2.Tag;

                if (current.Type != File_Type.Directory)
                    return;

                lbx.DataBind(SetCurrentDirectory());
            };

            btnNewFolder.MouseClick += async (s, e) => {
                var n = await new InputBox(EngineCore.Instance.Interface, "New Folder").Show().ConfigureAwait(false);
                if (string.IsNullOrEmpty(n))
                    return;

                var realPath = EngineFolders.ContentPathVirtualToReal(current.Path);
                Directory.CreateDirectory(Path.Combine(realPath, n));
                lbx.DataBind(SetCurrentDirectory());
            };

            btnNewFile.MouseClick += async (s, e) => {
                var n = await new NewTypeInputBox(EngineCore.Instance.Interface, current.Path).Show().ConfigureAwait(false);
                if (n)
                    lbx.DataBind(SetCurrentDirectory());
            };
        }

        private Dictionary<string, (File_Type, string)> SetCurrentDirectory()
        {
            var ds = new Dictionary<string, (File_Type, string)>();

            var realPath = EngineFolders.ContentPathVirtualToReal(current.Path);

            var dirs = Directory.GetDirectories(realPath);
            var files = Directory.GetFiles(realPath, searchPattern, SearchOption.TopDirectoryOnly).Where(x => !Path.GetFileName(x).StartsWith("."));

            foreach (var d in dirs)
                ds.Add(Path.GetFileName(d), (File_Type.Directory, EngineFolders.ContentPathRealToVirtual(d)));

            foreach (var d in files)
                ds.Add(Path.GetFileName(d), (File_Type.File, EngineFolders.ContentPathRealToVirtual(d)));

            txtPath.Text = string.IsNullOrEmpty(current.Path) ? "/" : current.Path;

            return ds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<(File_Type Type, string Path)> Show()
        {
            (File_Type Type, string Path) result = default;

            btnClose.MouseClick += (s, e) => {
                result = default;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            btnOpen.MouseClick += (s, e) => {
                result = current;
                ui.SetMainForm(previousForm);
                this.waitHandle.Set();
            };

            ui.SetMainForm(form);

            await Task.Run(() => waitHandle.WaitOne());
            return result;
        }
    }
}
