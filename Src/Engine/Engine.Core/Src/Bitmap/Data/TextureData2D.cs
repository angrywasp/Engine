using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;

namespace Engine.Bitmap.Data
{
    public class TextureData2D : IEquatable<TextureData2D>
    {
        //cache any previous conversion to TextureData
        private TextureData textureData;

        private readonly int width;
        private readonly int height;
        private readonly SurfaceFormat format;

        public int Width => width;
        public int Height => height;
        public SurfaceFormat Format => format;

        public byte[,] Red;
        public byte[,] Green;
        public byte[,] Blue;
        public byte[,] Alpha;

        public TextureData2D(int width, int height, SurfaceFormat format)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public static TextureData2D FromBitmap(SKBitmap bitmap)
        {
            var td = new TextureData2D(bitmap.Width, bitmap.Height, SkiaBitmap.SKColorTypeToSurfaceFormat(bitmap.ColorType));

            switch (td.Format)
            {
                case SurfaceFormat.Red:
                    td.Red = new byte[bitmap.Width, bitmap.Height];
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var pix = bitmap.GetPixel(x, y);
                            td.Red[x, y] = pix.Red;
                        }
                    }
                    break;
                case SurfaceFormat.Rg:
                    td.Red = new byte[bitmap.Width, bitmap.Height];
                    td.Green = new byte[bitmap.Width, bitmap.Height];
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var pix = bitmap.GetPixel(x, y);
                            td.Red[x, y] = pix.Red;
                            td.Green[x, y] = pix.Green;
                        }
                    }
                    break;
                case SurfaceFormat.Rgb:
                    td.Red = new byte[bitmap.Width, bitmap.Height];
                    td.Green = new byte[bitmap.Width, bitmap.Height];
                    td.Blue = new byte[bitmap.Width, bitmap.Height];
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var pix = bitmap.GetPixel(x, y);
                            td.Red[x, y] = pix.Red;
                            td.Green[x, y] = pix.Green;
                            td.Blue[x, y] = pix.Blue;
                        }
                    }
                    break;
                case SurfaceFormat.Rgba:
                    td.Red = new byte[bitmap.Width, bitmap.Height];
                    td.Green = new byte[bitmap.Width, bitmap.Height];
                    td.Blue = new byte[bitmap.Width, bitmap.Height];
                    td.Alpha = new byte[bitmap.Width, bitmap.Height];
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var pix = bitmap.GetPixel(x, y);
                            td.Red[x, y] = pix.Red;
                            td.Green[x, y] = pix.Green;
                            td.Blue[x, y] = pix.Blue;
                            td.Alpha[x, y] = pix.Alpha;
                        }
                    }

                    break;

                default: throw new FormatException("Invalid surface format");
            }

            return td;
        }

        public static TextureData2D Default(Color color, int width, int height)
        {
            var td = new TextureData2D(width, height, SurfaceFormat.Rgba);

            td.Fill(TextureData_Channel.Red, color.R);
            td.Fill(TextureData_Channel.Green, color.G);
            td.Fill(TextureData_Channel.Blue, color.B);
            td.Fill(TextureData_Channel.Alpha, color.A);

            return td;
        }

        public static byte[,] ExtractSingleChannel(SKBitmap bitmap)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            byte[,] pixData = new byte[bitmap.Width, bitmap.Height];

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pix = bitmap.GetPixel(x, y);
                    pixData[x, y] = pix.Red;
                }
            }

            sw.Stop();
            long ms2 = sw.ElapsedMilliseconds;

            return pixData;
        }

        public Color GetPixel(int x, int y)
        {
            switch (Format)
            {
                case SurfaceFormat.Red: return new Color(Red[x, y], (byte)0, (byte)0, (byte)255);
                case SurfaceFormat.Rg: return new Color(Red[x, y], Green[x, y], (byte)0, (byte)255);
                case SurfaceFormat.Rgb: return new Color(Red[x, y], Green[x, y], Blue[x, y], (byte)255);
                case SurfaceFormat.Rgba: return new Color(Red[x, y], Green[x, y], Blue[x, y], Alpha[x, y]);
                default: throw new FormatException("Invalid surface format");
            }
        }

        public void Fill(TextureData_Channel channel, byte color)
        {
            switch (channel)
            {
                case TextureData_Channel.Red:
                    Red = new byte[Width, Height];
                    for (int y = 0; y < this.Height; y++)
                        for (int x = 0; x < this.Width; x++)
                            Red[x, y] = color;
                    break;
                case TextureData_Channel.Green:
                    Green = new byte[Width, Height];
                    for (int y = 0; y < this.Height; y++)
                        for (int x = 0; x < this.Width; x++)
                            Green[x, y] = color;
                    break;
                case TextureData_Channel.Blue:
                    Blue = new byte[Width, Height];
                    for (int y = 0; y < this.Height; y++)
                        for (int x = 0; x < this.Width; x++)
                            Blue[x, y] = color;
                    break;
                case TextureData_Channel.Alpha:
                    Alpha = new byte[Width, Height];
                    for (int y = 0; y < this.Height; y++)
                        for (int x = 0; x < this.Width; x++)
                            Alpha[x, y] = color;
                    break;
            }
        }

        public void CopyChannel(TextureData_Channel channel, byte[,] source)
        {
            switch (channel)
            {
                case TextureData_Channel.Red:
                    Red = (byte[,])source.Clone();
                    break;
                case TextureData_Channel.Green:
                    Green = (byte[,])source.Clone();
                    break;
                case TextureData_Channel.Blue:
                    Blue = (byte[,])source.Clone();
                    break;
                case TextureData_Channel.Alpha:
                    Alpha = (byte[,])source.Clone();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void InvertChannel(TextureData_Channel channel)
        {
            byte[,] c;
            switch (channel)
            {
                case TextureData_Channel.Red:
                    c = Red;
                    break;
                case TextureData_Channel.Green:
                    c = Green;
                    break;
                case TextureData_Channel.Blue:
                    c = Blue;
                    break;
                case TextureData_Channel.Alpha:
                    c = Alpha;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            for (int x = 0; x < c.GetLength(0); x++)
                for (int y = 0; y < c.GetLength(1); y++)
                    c[x, y] = (byte)(255 - c[x, y]);
        }

        public TextureData ToTextureData(bool forceRefresh = false)
        {
            if (!forceRefresh && textureData != null)
                return textureData;

            var t = new TextureData(this.Width, this.Height, this.Format);

            int i = 0;

            switch (this.Format)
            {
                case SurfaceFormat.Red:
                    t.Red = new byte[this.Width * this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[i] = this.Red[x, y];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rg:
                    t.Red = new byte[this.Width * this.Height];
                    t.Green = new byte[this.Width * this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[i] = this.Red[x, y];
                            t.Green[i] = this.Green[x, y];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rgb:
                    t.Red = new byte[this.Width * this.Height];
                    t.Green = new byte[this.Width * this.Height];
                    t.Blue = new byte[this.Width * this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[i] = this.Red[x, y];
                            t.Green[i] = this.Green[x, y];
                            t.Blue[i] = this.Blue[x, y];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rgba:
                    t.Red = new byte[this.Width * this.Height];
                    t.Green = new byte[this.Width * this.Height];
                    t.Blue = new byte[this.Width * this.Height];
                    t.Alpha = new byte[this.Width * this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[i] = this.Red[x, y];
                            t.Green[i] = this.Green[x, y];
                            t.Blue[i] = this.Blue[x, y];
                            t.Alpha[i] = this.Alpha[x, y];
                            i++;
                        }
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            textureData = t;
            return t;
        }

        public void SaveBitmap(string path)
        {
            SKBitmap bmp = new SKBitmap(Width, Height, SkiaBitmap.SurfaceFormatToSKColorType(Format), SKAlphaType.Unpremul);
            var colors = new SKColor[Height * Width];
            int i = 0;
            switch (Format)
            {
                case SurfaceFormat.Red:
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                            colors[i++] = new SKColor(Red[x, y], 0, 0, 255);
                    break;
                case SurfaceFormat.Rg:
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                            colors[i++] = new SKColor(Red[x, y], Green[x, y], 0, 255);
                    break;
                case SurfaceFormat.Rgb:
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                            colors[i++] = new SKColor(Red[x, y], Green[x, y], Blue[x, y], 255);
                    break;
                case SurfaceFormat.Rgba:
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                            colors[i++] = new SKColor(Red[x, y], Green[x, y], Blue[x, y], Alpha[x, y]);
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            bmp.Pixels = colors;
            
            var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                fs.Write(data.AsSpan());
        }

        public List<TextureData2D> GenerateMipMaps()
        {
            var mipmaps = new List<TextureData2D>();
            int w = Width, h = Height;

            if (w == 1 || h == 1)
                mipmaps.Add(this);
            else
            {
                var mipmap = this;

                while (w > 1 && h > 1)
                {
                    mipmaps.Add(mipmap);
                    mipmap = ResizeHalf(mipmap);
                    w = mipmap.Width;
                    h = mipmap.Height;
                }
            }

            return mipmaps;
        }

        public TextureData2D ResizeHalf(TextureData2D previous)
        {
            var mipmap = new TextureData2D(previous.Width / 2, previous.Height / 2, previous.Format);

            switch (previous.Format)
            {
                case SurfaceFormat.Red:
                    mipmap.Red = new byte[mipmap.Width, mipmap.Height];
                    for (int y = 0; y < previous.Height; y += 2)
                    {
                        for (int x = 0; x < previous.Width; x += 2)
                        {
                            int x2 = x / 2, y2 = y / 2;
                            var pix0 = previous.GetPixel(x, y);
                            var pix1 = previous.GetPixel(x + 1, y);
                            var pix2 = previous.GetPixel(x, y + 1);
                            var pix3 = previous.GetPixel(x + 1, y + 1);
                            mipmap.Red[x2, y2] = (byte)((pix0.R + pix1.R + pix2.R + pix3.R) / 4);
                        }
                    }
                    break;
                case SurfaceFormat.Rg:
                    mipmap.Red = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Green = new byte[mipmap.Width, mipmap.Height];
                    for (int y = 0; y < previous.Height; y += 2)
                    {
                        for (int x = 0; x < previous.Width; x += 2)
                        {
                            int x2 = x / 2, y2 = y / 2;
                            var pix0 = previous.GetPixel(x, y);
                            var pix1 = previous.GetPixel(x + 1, y);
                            var pix2 = previous.GetPixel(x, y + 1);
                            var pix3 = previous.GetPixel(x + 1, y + 1);
                            mipmap.Red[x2, y2] = (byte)((pix0.R + pix1.R + pix2.R + pix3.R) / 4);
                            mipmap.Green[x2, y2] = (byte)((pix0.G + pix1.G + pix2.G + pix3.G) / 4);
                        }
                    }
                    break;
                case SurfaceFormat.Rgb:
                    mipmap.Red = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Green = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Blue = new byte[mipmap.Width, mipmap.Height];
                    for (int y = 0; y < previous.Height; y += 2)
                    {
                        for (int x = 0; x < previous.Width; x += 2)
                        {
                            int x2 = x / 2, y2 = y / 2;
                            var pix0 = previous.GetPixel(x, y);
                            var pix1 = previous.GetPixel(x + 1, y);
                            var pix2 = previous.GetPixel(x, y + 1);
                            var pix3 = previous.GetPixel(x + 1, y + 1);
                            mipmap.Red[x2, y2] = (byte)((pix0.R + pix1.R + pix2.R + pix3.R) / 4);
                            mipmap.Green[x2, y2] = (byte)((pix0.G + pix1.G + pix2.G + pix3.G) / 4);
                            mipmap.Blue[x2, y2] = (byte)((pix0.B + pix1.B + pix2.B + pix3.B) / 4);
                        }
                    }
                    break;
                case SurfaceFormat.Rgba:
                    mipmap.Red = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Green = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Blue = new byte[mipmap.Width, mipmap.Height];
                    mipmap.Alpha = new byte[mipmap.Width, mipmap.Height];
                    for (int y = 0; y < previous.Height; y += 2)
                    {
                        for (int x = 0; x < previous.Width; x += 2)
                        {
                            int x2 = x / 2, y2 = y / 2;
                            var pix0 = previous.GetPixel(x, y);
                            var pix1 = previous.GetPixel(x + 1, y);
                            var pix2 = previous.GetPixel(x, y + 1);
                            var pix3 = previous.GetPixel(x + 1, y + 1);
                            mipmap.Red[x2, y2] = (byte)((pix0.R + pix1.R + pix2.R + pix3.R) / 4);
                            mipmap.Green[x2, y2] = (byte)((pix0.G + pix1.G + pix2.G + pix3.G) / 4);
                            mipmap.Blue[x2, y2] = (byte)((pix0.B + pix1.B + pix2.B + pix3.B) / 4);
                            mipmap.Alpha[x2, y2] = (byte)((pix0.A + pix1.A + pix2.A + pix3.A) / 4);
                        }
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            return mipmap;
        }

        public override bool Equals(object obj) => Equals(obj as TextureData2D);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Red.GetHashCode();
                hashCode = (hashCode * 397) ^ Green.GetHashCode();
                hashCode = (hashCode * 397) ^ Blue.GetHashCode();
                hashCode = (hashCode * 397) ^ Alpha.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(TextureData2D other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            if (this.Red.Length != other.Red.Length)
                return false;

            if (this.Green.Length != other.Green.Length)
                return false;

            if (this.Blue.Length != other.Blue.Length)
                return false;

            if (this.Alpha.Length != other.Alpha.Length)
                return false;

            if (this.GetHashCode() != other.GetHashCode())
                return false;

            return true;
        }
    }
}
