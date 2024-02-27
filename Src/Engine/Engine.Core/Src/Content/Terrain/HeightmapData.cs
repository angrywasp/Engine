using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;

namespace Engine.Content.Terrain
{
    public class HeightmapData
    {
        private float[,] heightData;
        private VertexPositionTexture[,] positions;
        private VertexNormalTangentBinormal[,] normals;
        //private int[] indices;
        private int width;
        private int height;

        public float[,] HeightData => heightData;

        public VertexPositionTexture[,] Positions => positions;

        public VertexNormalTangentBinormal[,] Normals => normals;

        //public int[] Indices => indices;

        public int Width => width;

        public int Height => height;

        public static HeightmapData Create(float[,] heightData, VertexPositionTexture[,] positions, VertexNormalTangentBinormal[,] normals, int w, int h, GraphicsDevice gd)
        {
            HeightmapData td = new HeightmapData();
            td.heightData = heightData;
            td.positions = positions;
            td.normals = normals;

            td.width = w;
            td.height = h;

            return td;
        }

        public float CalculateSteepness(int x, int y)
        {
            float slopeX = HeightData[x < Width - 1 ? x + 1 : x, y] - HeightData[x > 0 ? x - 1 : x, y];
            float slopeZ = HeightData[x, y < Height - 1 ? y + 1 : y] - HeightData[x, y > 0 ? y - 1 : y];
 
            if (x == 0 || x == Width - 1)
                slopeX *= 2;
            if (y == 0 || y == Height - 1)
                slopeZ *= 2;
 
            Vector3 normal = Vector3.Normalize(new Vector3(-slopeX, 2, slopeZ));
            return (float)System.Math.Acos(Vector3.Dot(normal, Vector3.UnitY));
        }

        public void Unload()
        {
            heightData = null;
            positions = null;
            normals = null;
        }
    }
}