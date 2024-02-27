using System.IO;
using System.Linq;
using AngryWasp.Logger;
using Engine.Bitmap;
using Engine.Bitmap.Data;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MaterialPacker.Packers
{
    /// <summary>
    /// Generates an emissive texture from an input texture,
    /// if -emissiveFactor is set, overwrites the alpha channel with the content of the emissive factor file
    /// </summary>
    public class EmissivePacker
    {
        private CommandLine cl;

        public void Process(CommandLine cl)
        {
            this.cl = cl;

            Log.Instance.Write($"Packing Emissive texture");

            TextureData2D[] t = new TextureData2D[2];
            TextureData2D packed = TextureData2D.Default(new Color(0, 0, 0, 255), 1, 1);

            if (File.Exists(cl.Emissive))
                t[0] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Emissive, cl.Width, cl.Height));

            if (File.Exists(cl.EmissiveFactor))
                t[1] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.EmissiveFactor, cl.Width, cl.Height));

            var sizes = t.Where(x => x != null).Select(x => new Vector2i(x.Width, x.Height)).Distinct().ToArray();

            if (sizes.Length > 1)
                Log.Instance.WriteFatal("Maps provided to EmissivePacker are not the same size");
            else if (sizes.Length == 1)
            {
                packed = new TextureData2D(sizes[0].X, sizes[0].Y, SurfaceFormat.Rgba);

                if (t[0] != null)
                {
                    packed.CopyChannel(TextureData_Channel.Red, t[0].Red);
                    packed.CopyChannel(TextureData_Channel.Green, t[0].Green);
                    packed.CopyChannel(TextureData_Channel.Blue, t[0].Blue);
                }

                if (t[1] != null)
                        packed.CopyChannel(TextureData_Channel.Alpha, t[0].Red);
                    else
                        packed.Fill(TextureData_Channel.Alpha, 255);
            }

            App.WriteTexture(Texture_Type.Emissive, cl, packed);
        }
    }
}