using System.IO;
using Engine.Content.Terrain;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.AssetTransport
{
    public static class HeightmapReader
    {
        public static HeightmapData Read(GraphicsDevice graphicsDevice, byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data, false)))
            {
                int width = br.ReadInt32();
                int height = br.ReadInt32();

                float[,] heightData = new float[width, height];
                VertexPositionTexture[,] positions = new VertexPositionTexture[width, height];
                VertexNormalTangentBinormal[,] normals = new VertexNormalTangentBinormal[width, height];

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        heightData[x, y] = br.ReadSingle();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        positions[x, y] = br.ReadPositionVertex();
                        normals[x, y] = br.ReadNormalVertex();
                    }
                }

                return HeightmapData.Create(heightData, positions, normals, width, height, graphicsDevice);
            }
        }
    }
}
