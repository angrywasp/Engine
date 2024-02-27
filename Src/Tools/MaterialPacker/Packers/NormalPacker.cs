using System.IO;
using AngryWasp.Logger;
using Engine.Bitmap;
using Engine.Bitmap.Data;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework;

namespace MaterialPacker.Packers
{
    /// <summary>
    /// Generates a normal texture from an input texture
    /// --invertNormal inverts the green channel to donvert directx normals to opengl normals
    /// </summary>
    public class NormalPacker
    {
        private CommandLine cl;

        public void Process(CommandLine cl)
        {
            this.cl = cl;

            Log.Instance.Write($"Packing Normal texture");

            TextureData2D packed = TextureData2D.Default(new Color(128, 128, 255, 255), 1, 1);

            if (File.Exists(cl.Normal))
                packed = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Normal, cl.Width, cl.Height));
     
            if (cl.InvertNormal)
                packed.InvertChannel(TextureData_Channel.Green);

            App.WriteTexture(Texture_Type.Normal, cl, packed);
        }
    }
}