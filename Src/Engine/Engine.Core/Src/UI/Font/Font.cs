using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.UI
{
    public class Font
    {
        private byte[] textureData;
        private Vector2i textureSize;
        private Texture2D texture;
        private int fontSize;
        private Dictionary<char, FontChar> chars;

        public Texture2D Texture => texture;

        public int FontSize => fontSize;

        public FontChar this[char letter] => chars[letter];

        public int LetterSpacing { get; set; }

        public static Font Create(byte[] textureData, int fontSize, Vector2i textureSize, Dictionary<char, FontChar> chars)
        {
            Font f = new Font();
            f.textureData = textureData;
            f.textureSize = textureSize;
            f.chars = chars;
            f.fontSize = fontSize;

            return f;
        }

        public Vector2i MeasureChar(char ch)
        {
            FontChar fc;
            if (!chars.TryGetValue(ch, out fc))
                return Vector2i.Zero;

            return fc.Size;
        }

        public Vector2i MeasureString(string text)
        {
            char[] ch = text.ToCharArray();
            int x = 0, y = 0;
            foreach (var c in ch)
            {
                FontChar fc;
                if (!chars.TryGetValue(c, out fc))
                    continue;

                if (fc.Size.Y > y)
                    y = fc.Size.Y;

                x += fc.Size.X + LetterSpacing;
            }

            x -= LetterSpacing;
            if (x < 0)
                x = 0;

            return new Vector2i(x, y);
        }

        internal void CreateTexture(GraphicsDevice graphicsDevice)
        {
            //this method may return before texture is populated. this is fine and will just start rendering when it is ready
            Threading.BlockOnUIThread(() =>
            {
                texture = new Texture2D(graphicsDevice, textureSize.X, textureSize.Y, false, SurfaceFormat.Rgba);
                texture.SetData<byte>(textureData);
            });
        }
    }
}