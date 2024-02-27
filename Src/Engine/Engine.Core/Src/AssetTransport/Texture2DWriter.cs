using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Engine.Bitmap.Data;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public class Texture2DWriter
    {
        public static async Task<byte[]> Write(Texture2D tex)
        {
            byte[] returnData = null;

            await new AsyncUiTask().Run(() =>
            {
                using (var bw = new BinaryWriter(new MemoryStream()))
                {
                    bw.Write(tex.Width);
                    bw.Write(tex.Height);
                    bw.Write((byte)tex.Format);
                    bw.Write((byte)tex.LevelCount);

                    for (int m = 0; m < tex.LevelCount; m++)
                    {
                        Texture.GetSizeForLevel(tex.Width, tex.Height, m, out int levelWidth, out int levelHeight);
                        byte[] data = new byte[levelWidth * levelHeight * (int)tex.Format];
                        tex.GetData(m, data, 0, data.Length);

                        bw.Write(levelWidth);
                        bw.Write(levelHeight);
                        bw.Write(data);
                    }

                    returnData = ((MemoryStream)bw.BaseStream).ToArray();
                }
            }).ConfigureAwait(false);

            return returnData;
        }

        public static byte[] Write(List<TextureData2D> texData)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(texData[0].Width);
                bw.Write(texData[0].Height);
                bw.Write((byte)texData[0].Format);
                bw.Write((byte)texData.Count);

                for (int m = 0; m < texData.Count; m++)
                {
                    bw.Write(texData[m].Width);
                    bw.Write(texData[m].Height);
                    bw.Write(texData[m].ToTextureData().Interleave());
                }

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        public static byte[] Write(int width, int height, SurfaceFormat format, byte[] texData)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(width);
                bw.Write(height);
                bw.Write((byte)format);
                bw.Write((byte)1);

                bw.Write(width);
                bw.Write(height);
                bw.Write(texData);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }
    }
}
