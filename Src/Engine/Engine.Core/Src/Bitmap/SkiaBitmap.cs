using SkiaSharp;
using AngryWasp.Logger;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Bitmap
{
    public static class SkiaBitmap
    {
        public static SKBitmap Load(string path, int? width = null, int? height = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var bmp = SKBitmap.Decode(path);
            int rw = width.HasValue ? width.Value : bmp.Width;
            int rh = height.HasValue ? height.Value : bmp.Height;

            if (rw != bmp.Width || rh != bmp.Height)
            {
                Log.Instance.WriteWarning($"Bitmap {path} resized: {bmp.Width}x{bmp.Height} -> {rw}x{rh}");
                return bmp.Resize(new SKSizeI(rw, rh), SKFilterQuality.High);
            }
            else
                return bmp;
        }

        public static SKBitmap Load(byte[] imgData)
        {
            using (var ms = new MemoryStream(imgData))
                return SKBitmap.Decode(ms);
        }

        internal static SurfaceFormat SKColorTypeToSurfaceFormat(SKColorType colorType)
        {
            switch (colorType)
            {
                case SKColorType.Gray8:
                    return SurfaceFormat.Red;
                case SKColorType.Rg88:
                    return SurfaceFormat.Rg;
                case SKColorType.Rgb888x:
                    return SurfaceFormat.Rgb;
                case SKColorType.Rgba8888:
                case SKColorType.Bgra8888:
                    return SurfaceFormat.Rgba;
                case SKColorType.Rg1616:
                    return SurfaceFormat.Rg32;
                case SKColorType.Rgba16161616:
                    return SurfaceFormat.Rgba64;
                case SKColorType.RgF16:
                    return SurfaceFormat.HalfVector2;
                case SKColorType.RgbaF16:
                    return SurfaceFormat.HalfVector4;
                case SKColorType.RgbaF32:
                    return SurfaceFormat.Vector4;
                default:
                    throw new Exception($"SKColorType {colorType} does not map to a SurfaceFormat");
            }
        }

        internal static SKColorType SurfaceFormatToSKColorType(SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Red:
                    return SKColorType.Gray8;
                case SurfaceFormat.Rg:
                    return SKColorType.Rg88;
                case SurfaceFormat.Rgb:
                    return SKColorType.Rgb888x;
                case SurfaceFormat.Rgba:
                    return SKColorType.Rgba8888;
                case SurfaceFormat.Rg32:
                    return SKColorType.Rg1616;
                case SurfaceFormat.Rgba64:
                    return SKColorType.Rgba16161616;
                case SurfaceFormat.HalfVector2:
                    return SKColorType.RgF16;
                case SurfaceFormat.HalfVector4:
                    return SKColorType.RgbaF16;
                case SurfaceFormat.Vector4:
                    return SKColorType.RgbaF32;
                default:
                    throw new Exception($"SurfaceFormat {surfaceFormat} does not map to a SKColorType");
            }
        }
    }
}
