using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.Content;
using System.Threading.Tasks;

namespace Engine.Editor.Views
{
    public class FontView : View
    {
        private Vector2i position;
        private Vector2i size;

        private Font asset;

        public override void InitializeView(string path)
        {
            asset = ContentLoader.LoadFont(engine.GraphicsDevice, path);
            Helpers.ResizeAndPositionTexture(new Vector2i(asset.Texture.Width, asset.Texture.Height), engine.Interface.Size.ToVector2(), out position, out size);
            EditorGame.Instance.Window.ClientSizeChanged += (s, e) =>
            {
                Helpers.ResizeAndPositionTexture(new Vector2i(asset.Texture.Width, asset.Texture.Height), engine.Interface.Size.ToVector2(), out position, out size);
            };
        }

        public override void Draw(GameTime gameTime)
        { 
            //override with an empty method to prevent the engine from rendering
            engine.GraphicsDevice.SetRenderTarget(null);
            engine.GraphicsDevice.Clear(GraphicsDevice.DiscardDefault);

            if (asset != null)
            {
                engine.Interface.DrawRectangle(Color.White, position, size);
                engine.Interface.DrawRectangle(asset.Texture, Color.Black, position, size);
            }
            engine.Interface.DrawString("<?>", Vector2i.Zero, asset, Color.Red);

            engine.Interface.Draw();
			engine.GraphicsDevice.SetRenderTarget(null);
        }
    }
}