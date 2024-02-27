using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Engine.Bitmap.Data;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public class TextureCubeWriter
    {
        public static async Task<byte[]> Write(TextureCube tex)
        {
            byte[] returnData = null;

            await new AsyncUiTask().Run(() =>
            {
                using (var bw = new BinaryWriter(new MemoryStream()))
                {
                    bw.Write(tex.Size);
                    bw.Write((byte)tex.Format);
                    bw.Write((byte)tex.LevelCount);

                    for (int i = 0; i < 6; i++)
                    {
                        for (int m = 0; m < tex.LevelCount; m++)
                        {
                            Texture.GetSizeForLevel(tex.Size, tex.Size, m, out int levelWidth, out int levelHeight);
                            byte[] data = new byte[levelWidth * levelHeight * (int)tex.Format];
                            tex.GetData(CubeMapFace.PositiveX + i, m, null, data, 0, data.Length);

                            bw.Write(levelWidth);
                            bw.Write(data);
                        }
                    }

                    returnData = ((MemoryStream)bw.BaseStream).ToArray();
                }
            }).ConfigureAwait(false);

            return returnData;
        }

        public static byte[] Write(
            List<TextureData2D> posX, List<TextureData2D> negX,
            List<TextureData2D> posY, List<TextureData2D> negY,
            List<TextureData2D> posZ, List<TextureData2D> negZ)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(posX[0].Width);
                bw.Write((byte)posX[0].Format);
                bw.Write((byte)posX.Count);

                WriteMipmap(bw, posX);
                WriteMipmap(bw, negX);
                WriteMipmap(bw, posY);
                WriteMipmap(bw, negY);
                WriteMipmap(bw, posZ);
                WriteMipmap(bw, negZ);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        private static void WriteMipmap(BinaryWriter bw, List<TextureData2D> mipmaps)
        {
            foreach (var m in mipmaps)
            {
                bw.Write(m.Width);
                bw.Write(m.ToTextureData().Interleave());
            }
        }

        public static byte[] Write(
            List<TextureData> posX, List<TextureData> negX,
            List<TextureData> posY, List<TextureData> negY,
            List<TextureData> posZ, List<TextureData> negZ)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(posX[0].Width);
                bw.Write((byte)posX[0].Format);
                bw.Write((byte)posX.Count);
                
                WriteMipmap(bw, posX);
                WriteMipmap(bw, negX);
                WriteMipmap(bw, posY);
                WriteMipmap(bw, negY);
                WriteMipmap(bw, posZ);
                WriteMipmap(bw, negZ);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        private static void WriteMipmap(BinaryWriter bw, List<TextureData> mipmaps)
        {
            foreach (var m in mipmaps)
            {
                bw.Write(m.Width);
                bw.Write(m.Interleave());
            }
        }
    }
}
