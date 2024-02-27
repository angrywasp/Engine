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
    public class PbrPacker
    {
        private CommandLine cl;

        public void Process(CommandLine cl)
        {
            this.cl = cl;

            Log.Instance.Write($"Packing PBR texture");

            TextureData2D[] t = new TextureData2D[5];
            TextureData2D packed = TextureData2D.Default(new Color(0, 255, 255, 0), 1, 1);

            if (File.Exists(cl.Pbr))
                t[0] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Pbr, cl.Width, cl.Height));
            
            if (File.Exists(cl.Metalness))
                t[1] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Metalness, cl.Width, cl.Height));

            if (File.Exists(cl.AO))
                t[2] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.AO, cl.Width, cl.Height));

            if (File.Exists(cl.Roughness))
                t[3] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Roughness, cl.Width, cl.Height));

            if (File.Exists(cl.Displacement))
                t[4] = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Displacement, cl.Width, cl.Height));
            else
            {
                if (cl.Displacement != null)
                    Log.Instance.WriteFatal($"Displacement texture {cl.Displacement} does not exist");
            }

            var sizes = t.Where(x => x != null).Select(x => new Vector2i(x.Width, x.Height)).Distinct().ToArray();

            if (sizes.Length > 1)
                Log.Instance.WriteFatal("Maps provided to PbrPacker are not the same size");
            else if (sizes.Length == 1)
            {
                packed = new TextureData2D(sizes[0].X, sizes[0].Y, SurfaceFormat.Rgba);

                if (t[0] != null)
                    packed = t[0];
                else
                {
                    if (t[1] != null)
                        packed.CopyChannel(TextureData_Channel.Red, t[1].Red);
                    else
                        packed.Fill(TextureData_Channel.Red, 0);

                    if (t[2] != null)
                        packed.CopyChannel(TextureData_Channel.Green, t[2].Red);
                    else
                        packed.Fill(TextureData_Channel.Green, 255);

                    if (t[3] != null)
                        packed.CopyChannel(TextureData_Channel.Blue, t[3].Red);
                    else
                        packed.Fill(TextureData_Channel.Blue, 255);

                    if (t[4] != null)
                        packed.CopyChannel(TextureData_Channel.Alpha, t[4].Red);
                    else
                        packed.Fill(TextureData_Channel.Alpha, 0);
                }
            }

            App.WriteTexture(Texture_Type.Pbr, cl, packed);
        }
    }
}