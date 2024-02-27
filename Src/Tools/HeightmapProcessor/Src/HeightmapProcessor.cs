using System.IO;
using Engine.Helpers;
using System.Numerics;
using Engine.Content;
using AngryWasp.Helpers;
using Engine.Graphics.Vertices;
using Engine.Bitmap.Data;
using Engine.Bitmap;
using Engine.AssetTransport;
using Engine.Content.Model;
using System;

namespace HeightmapProcessor
{
    public class Processor
    {
        public void Process(CommandLine cl)
        {
            var bmp = SkiaBitmap.Load(cl.Input);
            byte[] pixelData = TextureData.ExtractSingleChannel(bmp);
            int w = bmp.Width, h = bmp.Height;

            //contstruct height data
            float[,] heightData = new float[w, h];

            //normalize in 0-1 range
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    heightData[x, y] = ((float)pixelData[x + y * w] / 255.0f);

            WaterErosion erosionData = new WaterErosion(heightData, w, h)
            {
                Deposition = cl.Deposition,
                IterationCount = cl.ErosionIterations,
                SedimentCapacity = cl.SedimentCapacity,
                SoilSoftness = cl.SoilSoftness
            };

            erosionData.Build();

            //erode it
            Erode(erosionData, heightData, w, h);

            //smooth it
            Smooth(cl.SmoothingIterations, heightData, w, h);

            TerrainHeightMap heightMap = new TerrainHeightMap(w, h, heightData, cl.VerticalScale);

            var positions = CreateVertices(heightMap);
            var indices = CreateIndices(heightMap);
            var normals = CalculateNormals(positions, indices);
            var tangents = MeshUtils.CalculateTangents(positions, normals, indices);

            for (int i = 0; i < normals.Length; i++)
                normals[i].SetTangent(tangents[i]);

            Directory.CreateDirectory(Path.GetDirectoryName(cl.Output));
            using (var bw = new BinaryWriter(new FileStream(cl.Output, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(heightMap.Width);
                bw.Write(heightMap.Height);

                for (int x = 0; x < heightMap.Width; x++)
                    for (int y = 0; y < heightMap.Height; y++)
                        bw.Write(heightMap.Values[x, y]);

                for (int i = 0; i < positions.Length; i++)
                {
                    bw.Write(positions[i]);
                    bw.Write(normals[i]);
                }
            }
        }

        private VertexPositionTexture[] CreateVertices(TerrainHeightMap h)
        {
            VertexPositionTexture[] v = new VertexPositionTexture[h.Width * h.Height];

            for (int x = 0; x < h.Width; x++)
                for (int y = 0; y < h.Height; y++)
                {
                    v[x + y * h.Width] = new VertexPositionTexture
                    {
                        Position = new Vector3(x, h[x, y], y),
                        TexCoord = new Vector2((float)x / h.Width, (float)y / h.Height)
                    };
                }

            return v;
        }

        private VertexNormalTangentBinormal[] CalculateNormals(in Span<VertexPositionTexture> positions, in Span<int> indices)
        {
            VertexNormalTangentBinormal[] normals = new VertexNormalTangentBinormal[positions.Length];

            int a = 0, b = 0, c = 0;

            for (a = 0; a < positions.Length; a++)
                normals[a].Normal = new Vector3(0, 0, 0);

            for (b = 0; b < indices.Length; b += 3)
            {
                int index1 = indices[b + 0];
                int index2 = indices[b + 1];
                int index3 = indices[b + 2];

                Vector3 side1 = positions[index1].Position - positions[index3].Position;
                Vector3 side2 = positions[index1].Position - positions[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                normals[index1].Normal += normal;
                normals[index2].Normal += normal;
                normals[index3].Normal += normal;
            }

            for (c = 0; c < positions.Length; c++)
                normals[c].Normal = Vector3.Normalize(normals[c].Normal);

            return normals;
        }

        private Vector4[] CalculateTangents(in Span<VertexPositionTexture> positions, in Span<VertexNormalTangentBinormal> normals, in Span<int> indices)
        {
            var length = positions.Length;

            Vector3[] tan1 = new Vector3[length];
            Vector3[] tan2 = new Vector3[length];
            Vector4[] tangents = new Vector4[length];

            for (int a = 0; a < indices.Length; a += 3)
            {
                var i1 = indices[a + 0];
                var i2 = indices[a + 1];
                var i3 = indices[a + 2];
                Vector3 v1 = positions[i1].Position;
                Vector3 v2 = positions[i2].Position;
                Vector3 v3 = positions[i3].Position;
                Vector2 w1 = positions[i1].TexCoord;
                Vector2 w2 = positions[i2].TexCoord;
                Vector2 w3 = positions[i3].TexCoord;
                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;
                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;
                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (int a = 0; a < length; ++a)
            {
                Vector3 n = normals[a].Normal;
                Vector3 t = tan1[a];
                Vector3 tmp = Vector3.Normalize(t - n * Vector3.Dot(n, t));
                tangents[a] = new Vector4(tmp.X, tmp.Y, tmp.Z, (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f ? -1.0f : 1.0f));
            }

            return tangents;
        }

        private int[] CreateIndices(TerrainHeightMap h)
        {
            int[] i = new int[(h.Width - 1) * (h.Height - 1) * 6];
            int counter = 0;
            for (int y = 0; y < h.Height - 1; y++)
                for (int x = 0; x < h.Width - 1; x++)
                {
                    int lowerLeft = x + y * h.Width;
                    int lowerRight = (x + 1) + y * h.Width;
                    int topLeft = x + (y + 1) * h.Width;
                    int topRight = (x + 1) + (y + 1) * h.Width;

                    i[counter++] = lowerLeft;
                    i[counter++] = lowerRight;
                    i[counter++] = topLeft;

                    i[counter++] = lowerRight;
                    i[counter++] = topRight;
                    i[counter++] = topLeft;
                }

            return i;
        }

        private void Erode(WaterErosion we, float[,] heightData, int width, int height)
        {
            if (we.IterationCount == 0)
                return;

            switch (we.ErosionEdgeMethod)
            {
                case Erosion_Edge_Method.Zero:
                case Erosion_Edge_Method.One:
                    {
                        for (int y = 0; y < height; y++)
                            for (int x = 0; x < width; x++)
                            {
                                if ((x == 0 || x == width - 1) ||
                                (y == 0 || y == height - 1))
                                    heightData[x, y] = (int)we.ErosionEdgeMethod;
                            }
                    }
                    break;
                case Erosion_Edge_Method.Neighbour:
                    {

                        for (int y = 0; y < height; y++)
                            for (int x = 0; x < width; x++)
                            {
                                int xx = x;
                                int yy = y;

                                if (x == 0)
                                    xx = x + 1;
                                else if (x == width - 1)
                                    xx = x - 1;

                                if (y == 0)
                                    yy = y + 1;
                                else if (y == height - 1)
                                    yy = y - 1;

                                heightData[x, y] = heightData[xx, yy];
                            }
                    }
                    break;
                case Erosion_Edge_Method.Interpolated:
                    {
                        for (int y = 0; y < height; y++)
                            for (int x = 0; x < width; x++)
                            {
                                int xx1 = x, xx2 = x;
                                int yy1 = y, yy2 = y;

                                bool skipX = false, skipY = false;

                                if (x == 0)
                                {
                                    xx1 = x + 1;
                                    xx2 = x + 2;
                                }
                                else if (x == width - 1)
                                {
                                    xx1 = x - 1;
                                    xx2 = x - 2;
                                }
                                else
                                    skipX = true;


                                if (y == 0)
                                {
                                    yy1 = y + 1;
                                    yy2 = y + 2;
                                }
                                else if (y == height - 1)
                                {
                                    yy1 = y - 1;
                                    yy2 = y - 2;
                                }
                                else
                                    skipY = true;

                                if (skipX && skipY)
                                    continue;

                                float n1 = heightData[xx1, yy1];
                                float n2 = heightData[xx2, yy2];

                                float ff1 = MathHelper.GetRelativeInterpolationPoints(n1, n2, 1)[0];
                                float ff2 = MathHelper.GetRelativeInterpolationPoints(n2, n1, 1)[0];
                                float ff = ff1 <= ff2 ? n1 - ff1 : n1 + ff2;
                                heightData[x, y] = ff;
                            }
                    }
                    break;
            }
        }

        private void Smooth(int smoothingIterations, float[,] heightData, int width, int height)
        {
            if (smoothingIterations == 0)
                return;

            float[,] newHeightData;

            int size = width;
            for (int passes = smoothingIterations; passes > 0; --passes)
            {
                newHeightData = new float[size, size];

                for (int x = 0; x < size; ++x)
                {
                    for (int z = 0; z < size; ++z)
                    {
                        int adjacentSections = 0;
                        float sectionsTotal = 0.0f;

                        int xMinusOne = x - 1;
                        int zMinusOne = z - 1;
                        int xPlusOne = x + 1;
                        int zPlusOne = z + 1;
                        bool bAboveIsValid = zMinusOne > 0;
                        bool bBelowIsValid = zPlusOne < size;

                        // =================================================================
                        if (xMinusOne > 0)            // Check to left
                        {
                            sectionsTotal += heightData[xMinusOne, z];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the left
                            {
                                sectionsTotal += heightData[xMinusOne, zMinusOne];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the left
                            {
                                sectionsTotal += heightData[xMinusOne, zPlusOne];
                                ++adjacentSections;
                            }
                        }
                        // =================================================================

                        // =================================================================
                        if (xPlusOne < size)     // Check to right
                        {
                            sectionsTotal += heightData[xPlusOne, z];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the right
                            {
                                sectionsTotal += heightData[xPlusOne, zMinusOne];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the right
                            {
                                sectionsTotal += heightData[xPlusOne, zPlusOne];
                                ++adjacentSections;
                            }
                        }
                        // =================================================================

                        // =================================================================
                        if (bAboveIsValid)            // Check above
                        {
                            sectionsTotal += heightData[x, zMinusOne];
                            ++adjacentSections;
                        }
                        // =================================================================

                        // =================================================================
                        if (bBelowIsValid)    // Check below
                        {
                            sectionsTotal += heightData[x, zPlusOne];
                            ++adjacentSections;
                        }
                        // =================================================================

                        newHeightData[x, z] = (heightData[x, z] + (sectionsTotal / adjacentSections)) * 0.5f;
                    }
                }

                // Overwrite the HeightData info with our new smoothed info
                for (int x = 0; x < size; ++x)
                    for (int z = 0; z < size; ++z)
                        heightData[x, z] = newHeightData[x, z];
            }
        }
    }
}
