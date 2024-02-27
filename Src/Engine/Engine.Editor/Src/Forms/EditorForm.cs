using System;
using System.IO;
using System.Threading.Tasks;
using Engine.Editor.Components.UI;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public abstract class EditorForm : UiForm
    {
        private bool isVisible = false;
        public bool IsVisible => isVisible;

        protected EngineCore engine;

        public EditorForm(EngineCore engine) : base(engine.Interface)
        {
            this.engine = engine;
        }

        public virtual EditorForm Show()
        {
            ui.SetMainForm(this);
            isVisible = true;
            return this;
        }

        public async Task<(bool ValidResult, string Path)> Browse(NodeType expectedNodeType)
        {
            var fileBrowserResult = await new FileBrowser(engine.Interface, $"*.{expectedNodeType.ToString().ToLower()}").Show().ConfigureAwait(false);
            if (fileBrowserResult.Type != FileBrowser.File_Type.File)
                return (false, null);

            var nodeType = Helpers.GetNodeTypeFromExtension(Path.GetExtension(fileBrowserResult.Path).Trim('.').ToLower());
            if (nodeType != expectedNodeType)
                return (false, null);

            string path = fileBrowserResult.Path;

            return (true, path);
        }

        public abstract bool ShouldUpdateCameraController();
        public abstract void ViewUpdate(GameTime gameTime);
        public abstract void ViewDraw();
    }
}