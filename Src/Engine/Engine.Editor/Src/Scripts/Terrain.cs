using Microsoft.Xna.Framework;
using Engine.Helpers;
using System.IO;
using AngryWasp.Logger;
using System;
using Engine.World.Objects;

namespace Engine.Editor.Forms.Scripts
{
    public static class TerrainScripts
    {
        private static HeightmapTerrain terrain;
        private static Color[] blendTextureData;
        private static int[,] blendTextureColorsArray2D;

        public static void Load(HeightmapTerrain t)
        {
            terrain = t;

            blendTextureData = new Color [terrain.BlendTexture.Width  * terrain.BlendTexture.Height];
            blendTextureColorsArray2D = new int[terrain.BlendTexture.Width, terrain.BlendTexture.Height];

            int z = 0;
            for (int y = 0; y < terrain.Heightmap.Height; y++)
                for (int x = 0; x < terrain.Heightmap.Width; x++)
                    blendTextureColorsArray2D[x, y] = z++;
        }

        public static void SetLayerBelowHeight(int index, float height)
        {
            try 
            {
                terrain.BlendTexture.GetData<Color>(blendTextureData);
                for (int y = 0; y < terrain.Heightmap.Height; y++)
                    for (int x = 0; x < terrain.Heightmap.Width; x++)
                    {
                        float h = terrain.Heightmap.HeightData[x, y];

                        if (h < height)
                        {
                            switch (index)
                            {
                                case 0:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].R = 255;
                                    break;
                                case 1:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].G = 255;
                                    break;
                                case 2:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].B = 255;
                                    break;
                                case 3:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].A = 255;
                                    break;
                            }
                        }
                    }

                terrain.BlendTexture.SetData<Color>(blendTextureData);
                terrain.Material.ApplyBlendTexture(terrain.BlendTexture);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteException(ex);
            }
        }

        public static void SetLayerAboveSteepness(int index, float steepness)
        {
            try
            {
                terrain.BlendTexture.GetData<Color>(blendTextureData);
                for (int y = 0; y < terrain.Heightmap.Height; y++)
                    for (int x = 0; x < terrain.Heightmap.Width; x++)
                    {
                        float h = terrain.Heightmap.CalculateSteepness(x, y);

                        if (h > steepness)
                        {
                            switch (index)
                            {
                                case 0:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].R = 255;
                                    break;
                                case 1:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].G = 255;
                                    break;
                                case 2:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].B = 255;
                                    break;
                                case 3:
                                    blendTextureData[blendTextureColorsArray2D[x, y]].A = 255;
                                    break;
                            }
                        }
                    }

                terrain.BlendTexture.SetData<Color>(blendTextureData);
                terrain.Material.ApplyBlendTexture(terrain.BlendTexture);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteException(ex);
            }
        }

        public static void Save()
        {
            byte[] rawData = new byte[blendTextureData.Length * 4];
            terrain.BlendTexture.GetData<byte>(rawData);

            using (var bw = new BinaryWriter(File.OpenWrite(EngineFolders.ContentPathVirtualToReal(terrain.Type.BlendTexture))))
            {
                bw.Write(terrain.BlendTexture.Width);
                bw.Write(terrain.BlendTexture.Height);
                bw.Write((byte)4);
                bw.Write((byte)0);
                bw.Write(rawData);
            }
        }

        public static void SaveAs(string path)
        {
            byte[] rawData = new byte[blendTextureData.Length * 4];
            terrain.BlendTexture.GetData<byte>(rawData);

            using (var bw = new BinaryWriter(File.OpenWrite(EngineFolders.ContentPathVirtualToReal(path))))
            {
                bw.Write(terrain.BlendTexture.Width);
                bw.Write(terrain.BlendTexture.Height);
                bw.Write((byte)4);
                bw.Write(rawData);
            }
        }
    }
}