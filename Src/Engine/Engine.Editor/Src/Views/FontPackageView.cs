using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.Content;
using Engine.Editor.Forms;
using System.Threading.Tasks;

namespace Engine.Editor.Views
{
    public class FontPackageView : View
    {
        private Font selectedFont;
        private Vector2i position;
        private Vector2i size;
        
        public override void InitializeView(string path)
        {
            FontPackage fontPack = ContentLoader.LoadFontPackage(engine.GraphicsDevice, path);
            selectedFont = fontPack.Smallest();

            var form = new FontPackageForm(engine, fontPack);
            form.SelectedFontChanged += (font) => { 
                selectedFont = font;
                Helpers.ResizeAndPositionTexture(new Vector2i(font.Texture.Width, font.Texture.Height), engine.Interface.Size.ToVector2(), out position, out size);
            };
            ShowForm(form);

            EditorGame.Instance.Window.ClientSizeChanged += (s, e) =>
            {
                if (selectedFont == null)
                    return;

                Helpers.ResizeAndPositionTexture(new Vector2i(selectedFont.Texture.Width, selectedFont.Texture.Height), engine.Interface.Size.ToVector2(), out position, out size);
            };
        }

        public override void Draw(GameTime gameTime)
        { 
            //override with an empty method to prevent the engine from rendering
            engine.GraphicsDevice.SetRenderTarget(null);
            engine.GraphicsDevice.Clear(GraphicsDevice.DiscardDefault);

            if (selectedFont != null)
                engine.Interface.DrawRectangle(selectedFont.Texture, Color.Black, position, size);

            engine.Interface.Draw();
			engine.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
