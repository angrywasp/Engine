using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public class TextureCubeReader
    {
        public static async Task<TextureCube> ReadAsync(GraphicsDevice graphicsDevice, byte[] data)
        {
            TextureCube tex = null;
            await new AsyncUiTask().Run(() => {
                tex = Read(graphicsDevice, data);
            }).ConfigureAwait(false);

            return tex;
        }

        public static TextureCube Read(GraphicsDevice graphicsDevice, byte[] data)
        {
            using (var fs = new MemoryStream(data))
            {
                using (var br = new BinaryReader(fs))
                {
                    int size = br.ReadInt32();
                    var format = (SurfaceFormat)br.ReadByte();
                    var levelCount = br.ReadByte();

                    var tex = new TextureCube(graphicsDevice, size, levelCount > 1, format);

                    for (int i = 0; i < 6; i++)
                    {
                        for (int m = 0; m < levelCount; m++)
                        {
                            size = br.ReadInt32();
                            int count = size * size * format.GetSize();
                            tex.SetData<byte>(CubeMapFace.PositiveX + i, m, null, br.ReadBytes(count), 0, count);
                        }
                    }

                    return tex;
                }
            }
        }

        public static async Task<List<TextureCube>> ReadMipmapsAsync(GraphicsDevice graphicsDevice, byte[] data)
        {
            Dictionary<int, TextureCube> mipmaps = new Dictionary<int, TextureCube>();

            await new AsyncUiTask().Run(() =>
            {
                using (var fs = new MemoryStream(data))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        int size = br.ReadInt32();
                        var format = (SurfaceFormat)br.ReadByte();
                        var levelCount = br.ReadByte();

                        for (int i = 0; i < 6; i++)
                        {
                            for (int m = 0; m < levelCount; m++)
                            {
                                size = br.ReadInt32();

                                if (!mipmaps.ContainsKey(size))
                                    mipmaps[size] = new TextureCube(graphicsDevice, size, false, format);

                                mipmaps[size].SetData<byte>(CubeMapFace.PositiveX + i, br.ReadBytes(size * size * format.GetSize()));
                            }
                        }
                    }
                }
            }).ConfigureAwait(false);

            return mipmaps.OrderByDescending(x => x.Key).Select(y => y.Value).ToList();
        }
    }
}
