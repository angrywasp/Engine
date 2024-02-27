using AngryWasp.Random;
using AngryWasp.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.Math
{
    public class Noise
    {
        private static XoShiRo256PlusPlus random = new XoShiRo256PlusPlus();

        public static byte[,] Generate(int width, int height, float scale)
        {
            float[,] texture = new float[width, height];
            byte[,] texBytes = new byte[width, height];

            List<Task> tasks = new List<Task>();

            double max = double.MinValue;
            double min = double.MaxValue;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var p = (float)Perlin(x * scale, y * scale);

                    if (p < min) min = p;
                    if (p > max) max = p;

                    texture[x, y] = p;
                }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    texBytes[x, y] = Normalize(texture[x, y], min, max);
            

            MakeSeamlessHorizontally(texture, width / 4);
            MakeSeamlessVertically(texture, height / 4);

            return texBytes;
        }

        public static byte Normalize(double value, double min = 0.0, double max = 1.0) => (byte)(((value - min) / (max - min)) * 255.0f);

        private static double Perlin(double x, double y)
        {
            double total = 0;
            double persistence = 0.5;
            int octaves = 8;
            for (int i = 0; i < octaves; i++)
            {
                double frequency = System.Math.Pow(2, i);
                double amplitude = System.Math.Pow(persistence, i);
                total += InterpolatedNoise(x * frequency, y * frequency) * amplitude;
            }
            return total;
        }

        private static double InterpolatedNoise(double x, double y)
        {
            int integerX = (int)x;
            double fractionalX = x - integerX;

            int integerY = (int)y;
            double fractionalY = y - integerY;

            double v1 = SmoothNoise(integerX, integerY);
            double v2 = SmoothNoise(integerX + 1, integerY);
            double v3 = SmoothNoise(integerX, integerY + 1);
            double v4 = SmoothNoise(integerX + 1, integerY + 1);

            double i1 = Interpolate(v1, v2, fractionalX);
            double i2 = Interpolate(v3, v4, fractionalX);

            return Interpolate(i1, i2, fractionalY);
        }

        private static double Interpolate(double a, double b, double x)
        {
            double ft = x * System.Math.PI;
            double f = (1 - System.Math.Cos(ft)) * 0.5;
            return a * (1 - f) + b * f;
        }

        private static double SmoothNoise(int x, int y)
        {
            double corners = (Random(x - 1, y - 1) + Random(x + 1, y - 1) + Random(x - 1, y + 1) + Random(x + 1, y + 1)) / 16;
            double sides = (Random(x - 1, y) + Random(x + 1, y) + Random(x, y - 1) + Random(x, y + 1)) / 8;
            double center = Random(x, y) / 4;
            return corners + sides + center;
        }

        private static double Random(int x, int y) => random.NextDouble();

        public static void MakeSeamlessHorizontally(float[,] noiseMap, int stitchWidth)
        {
            int width = noiseMap.GetUpperBound(0) + 1;
            int height = noiseMap.GetUpperBound(1) + 1;

            // iterate on the stitch band (on the left
            // of the noise)
            for (int x = 0; x < stitchWidth; x++)
            {
                // get the transparency value from
                // a linear gradient
                float v = x / (float)stitchWidth;
                for (int y = 0; y < height; y++)
                {
                    // compute the "mirrored x position":
                    // the far left is copied on the right
                    // and the far right on the left
                    int o = width - stitchWidth + x;
                    // copy the value on the right of the noise
                    noiseMap[o, y] = MathHelper.Lerp(noiseMap[o, y], noiseMap[stitchWidth - x, y], v);
                }
            }
        }

        public static void MakeSeamlessVertically(float[,] noiseMap, int stitchWidth)
        {
            int width = noiseMap.GetUpperBound(0) + 1;
            int height = noiseMap.GetUpperBound(1) + 1;

            // iterate through the stitch band (both
            // top and bottom sides are treated
            // simultaneously because its mirrored)
            for (int y = 0; y < stitchWidth; y++)
            {
                // number of neighbour pixels to
                // consider for the average (= kernel size)
                int k = stitchWidth - y;
                // go through the entire row
                for (int x = 0; x < width; x++)
                {
                    // compute the sum of pixel values
                    // in the top and the bottom bands
                    float s1 = 0.0f, s2 = 0.0f;
                    int c = 0;
                    for (int o = x - k; o < x + k; o++)
                    {
                        if (o < 0 || o >= width)
                            continue;
                        s1 += noiseMap[o, y];
                        s2 += noiseMap[o, height - y - 1];
                        c++;
                    }
                    // compute the means and assign them to
                    // the pixels in the top and the bottom
                    // rows
                    noiseMap[x, y] = s1 / (float)c;
                    noiseMap[x, height - y - 1] = s2 / (float)c;
                }
            }
        }
    }
}