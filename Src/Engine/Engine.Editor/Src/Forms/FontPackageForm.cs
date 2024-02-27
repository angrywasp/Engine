using System.Collections.Generic;
using System.Numerics;
using Engine.UI;
using Microsoft.Xna.Framework;

namespace Engine.Editor.Forms
{
    public class FontPackageForm : EditorForm
    {
        public delegate void SelectedFontChangedEventHandler(Font font);
        public event SelectedFontChangedEventHandler SelectedFontChanged;

        private FontPackage fontPackage;
        public FontPackageForm(EngineCore engine, FontPackage fontPackage) : base(engine)
        {
            this.fontPackage = fontPackage;
            ConstructLayout();
        }

        public override bool ShouldUpdateCameraController() => true;

        private void ConstructLayout()
        {
            var lbxFontSizes = this.ListBox(new Vector2(0.9f, 0), new Vector2(0.1f, 1));
            lbxFontSizes.SelectedItemChanged += (s, e) => {
                SelectedFontChanged?.Invoke(fontPackage.GetByIndex(lbxFontSizes.SelectedIndex));
            };

            var data = new Dictionary<string, object>();

            foreach (var f in fontPackage)
                data.Add(f.FontSize.ToString(), f.FontSize);

            lbxFontSizes.DataBind(data);
            lbxFontSizes.SelectedIndex = 0;
        }

        public override void ViewUpdate(GameTime gameTime) { }

        public override void ViewDraw() { }
    }
}