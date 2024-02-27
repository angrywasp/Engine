using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public class Texture2DReader
    {
        public static async Task<Texture2D> ReadAsync(GraphicsDevice graphicsDevice, byte[] data)
        {
            Texture2D tex = null;
            await new AsyncUiTask().Run(() => {
                tex = Read(graphicsDevice, data);
            }).ConfigureAwait(false);

            return tex;
        }

        public static Texture2D Read(GraphicsDevice graphicsDevice, byte[] data)
        {
            using (var fs = new MemoryStream(data))
            {
                using (var br = new BinaryReader(fs))
                {
                    var width = br.ReadInt32();
                    var height = br.ReadInt32();
                    var format = (SurfaceFormat)br.ReadByte();
                    var levelCount = br.ReadByte();

                    var tex = new Texture2D(graphicsDevice, width, height, levelCount > 1, format);

                    for (int m = 0; m < levelCount; m++)
                    {
                        width = br.ReadInt32();
                        height = br.ReadInt32();
                        int count = width * height * format.GetSize();
                        tex.SetData<byte>(m, br.ReadBytes(count), 0, count);
                    }

                    return tex;
                }
            }
        }

        public static async Task<List<Texture2D>> ReadMipmapsAsync(GraphicsDevice graphicsDevice, byte[] data)
        {
            List<Texture2D> mipmaps = new List<Texture2D>();

            await new AsyncUiTask().Run(() =>
            {
                using (var fs = new MemoryStream(data))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        var width = br.ReadInt32();
                        var height = br.ReadInt32();
                        var format = (SurfaceFormat)br.ReadByte();
                        var levelCount = br.ReadByte();

                        for (int m = 0; m < levelCount; m++)
                        {
                            width = br.ReadInt32();
                            height = br.ReadInt32();

                            Texture2D mm = new Texture2D(graphicsDevice, width, height, false, format);
                            mm.SetData<byte>(br.ReadBytes(width * height * format.GetSize()));
                            mipmaps.Add(mm);
                        }
                    }
                }
            }).ConfigureAwait(false);

            return mipmaps;
        }
    }
}
