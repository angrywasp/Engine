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
    /// Generates an albedo texture from an input texture,
    /// if --alphaMask is set, overwrites the alpha channel with the content of the alpha mask file
    /// </summary>
    public class AlbedoPacker
    {
        private CommandLine cl;

        public void Process(CommandLine cl)
        {
            this.cl = cl;

            Log.Instance.Write($"Packing Albedo texture");

            TextureData2D[] t = new TextureData2D[2];
            TextureData2D packed = TextureData2D.Default(new Color(128, 128, 128, 255), 1, 1);

            if (File.Exists(cl.Albedo))
                t[0] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Albedo, cl.Width, cl.Height));

            if (File.Exists(cl.AlphaMask))
                t[1] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.AlphaMask, cl.Width, cl.Height));

            var sizes = t.Where(x => x != null).Select(x => new Vector2i(x.Width, x.Height)).Distinct().ToArray();

            if (sizes.Length > 1)
                Log.Instance.WriteFatal("Maps provided to AlbnedoPacker are not the same size");
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

            App.WriteTexture(Texture_Type.Albedo, cl, packed);
        }
    }
}