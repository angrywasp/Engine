using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Engine.AssetTransport;
using Engine.Content;
using Engine.Helpers;
using Engine.PostProcessing.PostProcesses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Editor.Views
{
    public class TextureView : View
    {
        private Vector2i position;
        private Vector2i size;

        private ChannelFilter filter;

        int currentMipLevel = 0;

        List<Texture2D> asset;

        public override void InitializeView(string path)
        {
            Task.Run(async () => {
                asset = await Texture2DReader.ReadMipmapsAsync(engine.GraphicsDevice, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(path))).ConfigureAwait(false);
                Helpers.ResizeAndPositionTexture(new Vector2i(asset[currentMipLevel].Width, asset[currentMipLevel].Height), engine.Interface.Size.ToVector2(), out position, out size);
            });

            EditorGame.Instance.Window.ClientSizeChanged += (s, e) =>
            {
                Helpers.ResizeAndPositionTexture(new Vector2i(asset[currentMipLevel].Width, asset[currentMipLevel].Height), engine.Interface.Size.ToVector2(), out position, out size);
            };

            filter = new ChannelFilter();
            engine.PostProcessor.Add("ColorFilter", filter);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (asset == null || asset.Count == 0)
                return;

            engine.Interface.ScreenMessages.WriteStaticText(0, $" Total mipmap levels: {asset.Count}", Color.White);
            engine.Interface.ScreenMessages.WriteStaticText(1, $"Current mipmap level: {currentMipLevel}", Color.White);

            if (engine.Input.Keyboard.KeyJustPressed(Keys.R))
                filter.ShowRed = !filter.ShowRed;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.G))
                filter.ShowGreen = !filter.ShowGreen;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.B))
                filter.ShowBlue = !filter.ShowBlue;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.A))
                filter.ShowAlpha = !filter.ShowAlpha;

            int newMipLevel = currentMipLevel;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.Down))
                ++newMipLevel;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.Up))
                --newMipLevel;

            newMipLevel = MathHelper.Clamp(newMipLevel, 0, asset.Count - 1);

            if (newMipLevel != currentMipLevel)
            {
                currentMipLevel = newMipLevel;
                Helpers.ResizeAndPositionTexture(new Vector2i(asset[currentMipLevel].Width, asset[currentMipLevel].Height), engine.Interface.Size.ToVector2(), out position, out size);
            }
        }

        public override void Draw(GameTime gameTime)
        { 
            //override with an empty method to prevent the engine from rendering
            engine.GraphicsDevice.SetRenderTarget(null);
            engine.GraphicsDevice.Clear(GraphicsDevice.DiscardDefault);

            if (asset == null || asset.Count == 0)
                return;

            engine.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            engine.PostProcessor.Draw(asset[currentMipLevel]);
            engine.Interface.DrawRectangle(asset[currentMipLevel], Color.White, position, size, BlendState.AlphaBlend);
            engine.Interface.Draw();
			//engine.GraphicsDevice.SetRenderTarget(null);
        }
    }
}