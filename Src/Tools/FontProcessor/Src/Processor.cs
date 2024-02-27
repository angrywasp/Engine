using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngryWasp.Logger;
using Engine.UI;
using Microsoft.Xna.Framework;
using SkiaSharp;

namespace FontProcessor
{
    public class Processor
    {
        private List<char> letters = new List<char>();
        private List<int> sizes;

        public void Process(CommandLine cl)
        {
            sizes = cl.Sizes?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x.Trim())).ToList();

            if (cl.Numbers)
                letters.AddRange("0123456789".ToCharArray());

            if (cl.LowerCase)
                letters.AddRange("abcdefghijklmnopqrstuvwxyz".ToCharArray());

            if (cl.UpperCase)
                letters.AddRange("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());

            if (cl.Symbols)
                letters.AddRange("!@#$%^&*()-=_+{}[]:\";'<>?,./~\\|` ".ToCharArray());

            if (letters == null || letters.Count == 0 || sizes == null || sizes.Count == 0)
            {
                Log.Instance.WriteError("Nothing to process");
                return;
            }

            Log.Instance.Write($"Generating Font {Path.GetFileName(cl.Output)}");

            List<byte[]> fontBinaries = new List<byte[]>();
            foreach (var sz in sizes)
            {
                var fontData = WriteFont(cl, sz);
                File.WriteAllBytes(Path.ChangeExtension(cl.Output, sz.ToString() + ".font"), fontData);
                fontBinaries.Add(fontData);
            }

            using (var bw = new BinaryWriter(new FileStream(Path.ChangeExtension(cl.Output, ".fontpkg"), FileMode.Create, FileAccess.Write)))
            {
                bw.Write(fontBinaries.Count);

                foreach (var fb in fontBinaries)
                    bw.Write(fb.Length);

                foreach (var fb in fontBinaries)
                    bw.Write(fb);
            }
        }

        private byte[] WriteFont(CommandLine cl, int size)
        {
            var chars = GetChars(cl, size);
            var bitmap = SetChars(chars);

            var skImage = SKImage.FromBitmap(bitmap);
            File.WriteAllBytes(
                Path.ChangeExtension(cl.Output, size.ToString() + ".png"),
                skImage.Encode(SKEncodedImageFormat.Png, 100).ToArray()
            );

            var ms = new MemoryStream();

            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(size);
                bw.Write(chars.Count);
                bw.Write(cl.Spacing);
                bw.Write(bitmap.Width);
                bw.Write(bitmap.Height);

                foreach (var c in chars)
                {
                    bw.Write(c.Key);
                    bw.Write(c.Value.FontChar.Size.X);
                    bw.Write(c.Value.FontChar.Size.Y);
                    bw.Write(c.Value.FontChar.Position.X);
                    bw.Write(c.Value.FontChar.Position.Y);
                }

                bw.Write(bitmap.Bytes);
                skImage.Dispose();
                bitmap.Dispose();
                return ms.ToArray();
            }
        }

        private Dictionary<char, (SKBitmap Bitmap, FontChar FontChar)> GetChars(CommandLine cl, int fontSize)
        {
            var returnValue = new Dictionary<char, (SKBitmap Bitmap, FontChar FontChar)>();

            var family = SKTypeface.FromFile(cl.Input);
            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.White,
                TextSize = fontSize,
                TextAlign = SKTextAlign.Left,
                Typeface = family
            };

            foreach (var letter in letters)
            {
                var r = new SKRect();
                var x = paint.MeasureText(letter == ' ' ? "_" : letter.ToString(), ref r);

                var size = new SKSize((int)Math.Ceiling(x), (int)Math.Ceiling(paint.FontMetrics.Descent - paint.FontMetrics.Ascent + paint.FontMetrics.Leading));

                var bitmap = new SKBitmap((int)size.Width, (int)Math.Ceiling(size.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                var canvas = new SKCanvas(bitmap);

                canvas.Clear();
                canvas.DrawText(letter.ToString(), 0, (int)Math.Ceiling(-paint.FontMetrics.Ascent), paint);
                FontChar fc = new FontChar
                {
                    Position = Vector2i.Zero,
                    Size = new Vector2i((int)bitmap.Width, (int)bitmap.Height)
                };
                returnValue.Add(letter, (bitmap, fc));
                canvas.Dispose();
            }

            return returnValue;
        }

        private SKBitmap SetChars(Dictionary<char, (SKBitmap Bitmap, FontChar FontChar)> chars)
        {
            Vector2i texSize = GetTextureSize(ref chars);

            var bitmap = new SKBitmap(texSize.X, texSize.Y, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var canvas = new SKCanvas(bitmap);

            foreach (var c in chars)
            {
                if (c.Value.Bitmap.Width == 0 || c.Value.Bitmap.Height == 0)
                {
                    Log.Instance.Write($"Skipping char '{c.Key}'. Could not generate bitmap");
                    continue;
                }
                canvas.DrawBitmap(c.Value.Bitmap, new SKPoint(c.Value.FontChar.Position.X, c.Value.FontChar.Position.Y));
            }

            canvas.Dispose();
            return bitmap;
        }

        private Vector2i GetTextureSize(ref Dictionary<char, (SKBitmap Bitmap, FontChar FontChar)> fonts)
        {
            Vector2i t = fonts.First().Value.FontChar.Size;
            Vector2i returnValue;

            int count = fonts.Count;
            int sqrt = (int)Math.Round(Math.Sqrt(count), 0, MidpointRounding.ToEven);

            int tx = t.X * sqrt;

            int currentRowMaxHeight = 0;
            int offsetX = 0;
            int offsetY = 0;
            foreach (var f in fonts.Values)
            {
                if (offsetX + f.FontChar.Size.X > tx)
                {
                    //need a new row
                    offsetX = 0;
                    offsetY += currentRowMaxHeight;
                    currentRowMaxHeight = 0;
                }

                if (f.FontChar.Size.Y > currentRowMaxHeight)
                    currentRowMaxHeight = f.FontChar.Size.Y;

                f.FontChar.Position = new Vector2i(offsetX, offsetY);
                offsetX += f.FontChar.Size.X;
            }

            returnValue = new Vector2i(tx, offsetY + currentRowMaxHeight);
            return returnValue;
        }
    }
}