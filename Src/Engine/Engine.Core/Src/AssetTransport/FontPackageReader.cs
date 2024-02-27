using System.IO;
using Engine.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public static class FontPackageReader
    {
        public static FontPackage Read(GraphicsDevice graphicsDevice, byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                FontPackage fontPkg = new FontPackage();

                var numFonts = br.ReadInt32();
                var fontLengths = new int[numFonts];

                for (int i = 0; i < numFonts; i++)
                    fontLengths[i] = br.ReadInt32();

                for (int i = 0; i < numFonts; i++)
                {
                    byte[] fontData = new byte[fontLengths[i]];
                    br.Read(fontData);
                    
                    fontPkg.AddFont(FontReader.Read(graphicsDevice, fontData));
                }

                return fontPkg;
            }
        }
    }
}
