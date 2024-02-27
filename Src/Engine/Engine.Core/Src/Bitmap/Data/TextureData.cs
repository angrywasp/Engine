using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace Engine.Bitmap.Data
{
    public class TextureData : IEquatable<TextureData>
    {
        private TextureData2D textureData2D;
        private byte[] interleaved;

        private readonly int width;
        private readonly int height;
        private readonly SurfaceFormat format;

        public int Width => width;
        public int Height => height;
        public SurfaceFormat Format => format;
        
        public byte[] Red;
        public byte[] Green;
        public byte[] Blue;
        public byte[] Alpha;

        public TextureData(int width, int height, SurfaceFormat format)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public static TextureData FromBitmap(SKBitmap bitmap)
        {
            var td = new TextureData(bitmap.Width, bitmap.Height, SkiaBitmap.SKColorTypeToSurfaceFormat(bitmap.ColorType));
            byte[] data = new byte[bitmap.ByteCount];

            unsafe
            {
                byte* ptr = (byte*)bitmap.GetPixels().ToPointer();
                Marshal.Copy((IntPtr)ptr, data, 0, data.Length);
            }

            int x = 0;
            switch (bitmap.ColorType)
            {
                case SKColorType.Gray8:
                    td.Red = data;
                    break;
                case SKColorType.Rg88:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 2)
                    {
                        x = i / 2;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                    }
                    break;
                case SKColorType.Rgb888x:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    td.Blue = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 3)
                    {
                        x = i / 3;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                        td.Blue[x] = data[i + 2];
                    }
                    break;
                case SKColorType.Rgba8888:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    td.Blue = new byte[td.Height * td.Width];
                    td.Alpha = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 4)
                    {
                        x = i / 4;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                        td.Blue[x] = data[i + 2];
                        td.Alpha[x] = data[i + 3];
                    }
                    break;
                case SKColorType.Bgra8888:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    td.Blue = new byte[td.Height * td.Width];
                    td.Alpha = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 4)
                    {
                        x = i / 4;
                        td.Blue[x] = data[i];
                        td.Green[x] = data[i + 1];
                        td.Red[x] = data[i + 2];
                        td.Alpha[x] = data[i + 3];
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            return td;
        }

        public static TextureData FromTextureCube(TextureCube t, CubeMapFace f)
        {
            var td = new TextureData(t.Size, t.Size, t.Format);
            byte[] data = new byte[td.Width * td.Height * td.Format.GetSize()];
            t.GetData(f, data);

            return FromBytes(td, data);
        }

        public static TextureData FromTexture2D(Texture2D t)
        {
            var td = new TextureData(t.Width, t.Height, t.Format);
            byte[] data = new byte[td.Width * td.Height * td.Format.GetSize()];
            t.GetData(data);

            return FromBytes(td, data);
        }

        //Assumes data is rgba format
        public static TextureData FromBytes(TextureData td, byte[] data)
        {
            int x = 0;
            switch (td.Format)
            {
                case SurfaceFormat.Red:
                    td.Red = data;
                    break;
                case SurfaceFormat.Rg:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 2)
                    {
                        x = i / 2;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                    }
                    break;
                case SurfaceFormat.Rgb:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    td.Blue = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 3)
                    {
                        x = i / 3;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                        td.Blue[x] = data[i + 2];
                    }
                    break;
                case SurfaceFormat.Rgba:
                    td.Red = new byte[td.Height * td.Width];
                    td.Green = new byte[td.Height * td.Width];
                    td.Blue = new byte[td.Height * td.Width];
                    td.Alpha = new byte[td.Height * td.Width];
                    for (int i = 0; i < data.Length; i += 4)
                    {
                        x = i / 4;
                        td.Red[x] = data[i];
                        td.Green[x] = data[i + 1];
                        td.Blue[x] = data[i + 2];
                        td.Alpha[x] = data[i + 3];
                    }
                    break;
                case SurfaceFormat.HalfVector2:
                    {
                        td.Red = new byte[td.Height * td.Width * 2];
                        td.Green = new byte[td.Height * td.Width * 2];
                        int r = 0, g = 0;
                        for (int i = 0; i < data.Length; i += 8)
                        {
                            td.Red[r++] = data[i];
                            td.Red[r++] = data[i + 1];

                            td.Green[g++] = data[i + 2];
                            td.Green[g++] = data[i + 3];
                        }
                    }
                    break;
                case SurfaceFormat.Vector2:
                    {
                        td.Red = new byte[td.Height * td.Width * 4];
                        td.Green = new byte[td.Height * td.Width * 4];
                        int r = 0, g = 0;
                        for (int i = 0; i < data.Length; i += 8)
                        {
                            td.Red[r++] = data[i];
                            td.Red[r++] = data[i + 1];
                            td.Red[r++] = data[i + 2];
                            td.Red[r++] = data[i + 3];

                            td.Green[g++] = data[i + 4];
                            td.Green[g++] = data[i + 5];
                            td.Green[g++] = data[i + 6];
                            td.Green[g++] = data[i + 7];
                        }
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            return td;
        }

        public static TextureData Default(Color color, int width, int height)
        {
            var td = new TextureData(width, height, SurfaceFormat.Rgba);
            td.Fill(TextureData_Channel.Red, color.R);
            td.Fill(TextureData_Channel.Green, color.G);
            td.Fill(TextureData_Channel.Blue, color.B);
            td.Fill(TextureData_Channel.Alpha, color.A);

            return td;
        }

        public static byte[] ExtractSingleChannel(SKBitmap bitmap)
        {
            //6ms
            var pixData = bitmap.Pixels.Select(x => x.Red).ToArray();

            //96ms
            /*
            byte[] pixData = new byte[bitmap.Width * bitmap.Height];
            int i = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pix = bitmap.GetPixel(x, y);
                    pixData[i++] = pix.Red;
                }
            }
            */

            return pixData;
        }

        public void Fill(TextureData_Channel channel, byte color)
        {
            switch (channel)
            {
                case TextureData_Channel.Red:
                    Red = Enumerable.Repeat(color, Width * Height).ToArray();
                    break;
                case TextureData_Channel.Green:
                    Green = Enumerable.Repeat(color, Width * Height).ToArray();
                    break;
                case TextureData_Channel.Blue:
                    Blue = Enumerable.Repeat(color, Width * Height).ToArray();
                    break;
                case TextureData_Channel.Alpha:
                    Alpha = Enumerable.Repeat(color, Width * Height).ToArray();
                    break;
            }
        }

        public void CopyChannel(TextureData_Channel channel, byte[] source)
        {
            switch (channel)
            {
                case TextureData_Channel.Red:
                    Red = (byte[])source.Clone();
                    break;
                case TextureData_Channel.Green:
                    Green = (byte[])source.Clone();
                    break;
                case TextureData_Channel.Blue:
                    Blue = (byte[])source.Clone();
                    break;
                case TextureData_Channel.Alpha:
                    Alpha = (byte[])source.Clone();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public byte[] Interleave(bool forceRefresh = false)
        {
            if (!forceRefresh && interleaved != null)
                return interleaved;

            var bytes = new byte[Width * Height * Format.GetSize()];
            var x = 0;

            switch (Format)
            {
                case SurfaceFormat.Red:
                    bytes = Red;
                    break;
                case SurfaceFormat.Rg:
                    for (int i = 0; i < (Width * Height); i++)
                    {
                        bytes[x++] = Red[i];
                        bytes[x++] = Green[i];
                    }
                    break;
                case SurfaceFormat.Rgb:
                    for (int i = 0; i < (Width * Height); i++)
                    {
                        bytes[x++] = Red[i];
                        bytes[x++] = Green[i];
                        bytes[x++] = Blue[i];
                    }
                    break;
                case SurfaceFormat.Rgba:
                    for (int i = 0; i < (Width * Height); i++)
                    {
                        bytes[x++] = Red[i];
                        bytes[x++] = Green[i];
                        bytes[x++] = Blue[i];
                        bytes[x++] = Alpha[i];
                    }
                    break;
                case SurfaceFormat.HalfVector2:
                    for (int i = 0; i < (Width * Height * 2); i += 2)
                    {
                        bytes[x++] = Red[i];
                        bytes[x++] = Red[i + 1];

                        bytes[x++] = Green[i];
                        bytes[x++] = Green[i + 1];
                    }
                    break;
                case SurfaceFormat.Vector2:
                    for (int i = 0; i < (Width * Height * 4); i += 4)
                    {
                        bytes[x++] = Red[i];
                        bytes[x++] = Red[i + 1];
                        bytes[x++] = Red[i + 2];
                        bytes[x++] = Red[i + 3];

                        bytes[x++] = Green[i];
                        bytes[x++] = Green[i + 1];
                        bytes[x++] = Green[i + 2];
                        bytes[x++] = Green[i + 3];
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            interleaved = bytes;
            return bytes;
        }

        public void InvertChannel(TextureData_Channel channel)
        {
            byte[] c;
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

            c = c.Select(x => x = (byte)(255 - x)).ToArray();
        }

        public TextureData2D ToTextureData2D(bool forceRefresh = false)
        {
            if (!forceRefresh && textureData2D != null)
                return textureData2D;

            var t = new TextureData2D(this.Width, this.Height, this.Format);

            int i = 0;

            switch (Format)
            {
                case SurfaceFormat.Red:
                    t.Red = new byte[this.Width, this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[x, y] = this.Red[i];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rg:
                    t.Red = new byte[this.Width, this.Height];
                    t.Green = new byte[this.Width, this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[x, y] = this.Red[i];
                            t.Green[x, y] = this.Green[i];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rgb:
                    t.Red = new byte[this.Width, this.Height];
                    t.Green = new byte[this.Width, this.Height];
                    t.Blue = new byte[this.Width, this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[x, y] = this.Red[i];
                            t.Green[x, y] = this.Green[i];
                            t.Blue[x, y] = this.Blue[i];
                            i++;
                        }
                    }
                    break;
                case SurfaceFormat.Rgba:
                    t.Red = new byte[this.Width, this.Height];
                    t.Green = new byte[this.Width, this.Height];
                    t.Blue = new byte[this.Width , this.Height];
                    t.Alpha = new byte[this.Width, this.Height];
                    for (int y = 0; y < this.Height; y++)
                    {
                        for (int x = 0; x < this.Width; x++)
                        {
                            t.Red[x, y] = this.Red[i];
                            t.Green[x, y] = this.Green[i];
                            t.Blue[x, y] = this.Blue[i];
                            t.Alpha[x, y] = this.Alpha[i];
                            i++;
                        }
                    }
                    break;
                default: throw new FormatException("Invalid surface format");
            }

            textureData2D = t;
            return t;
        }
    
        public void SaveBitmap(string path)
        {
            SKBitmap bmp = new SKBitmap(Width, Height, SkiaBitmap.SurfaceFormatToSKColorType(Format), SKAlphaType.Unpremul);
            byte[] interleaved = Interleave();

            unsafe
            {
                fixed (byte* ptr = interleaved)
                    bmp.SetPixels((IntPtr)ptr);
            }
            
            var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                fs.Write(data.AsSpan());
        }

        public override bool Equals(object obj) => Equals(obj as TextureData);

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

        public bool Equals(TextureData other)
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
