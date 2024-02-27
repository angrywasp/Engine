using System.Collections.Generic;
using System.IO;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public static class FontReader
    {
        public static Font Read(GraphicsDevice graphicsDevice, byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                Dictionary<char, FontChar> chars = new Dictionary<char, FontChar>();

                var size = br.ReadInt32();
                var numChars = br.ReadInt32();
                var letterSpacing = br.ReadInt32();
                var texWidth = br.ReadInt32();
                var texHeight = br.ReadInt32();

                for (int i = 0; i < numChars; i++)
                {
                    var ch = br.ReadChar();
                    chars.Add(ch, new FontChar
                    {
                        Size = new Vector2i(br.ReadInt32(), br.ReadInt32()),
                        Position = new Vector2i(br.ReadInt32(), br.ReadInt32())
                    });
                }

                byte[] texData = new byte[texWidth * texHeight * 4];

                br.Read(texData);

                Font font = Font.Create(texData, size, new Vector2i(texWidth, texHeight), chars);
                font.CreateTexture(graphicsDevice);
                font.LetterSpacing = letterSpacing;

                return font;
            }
        }
    }
}
