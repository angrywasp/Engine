using System.Collections.Generic;
using System.IO;
using AngryWasp.Logger;
using Engine.AssetTransport;
using Engine.Bitmap;
using Engine.Bitmap.Data;

namespace TextureProcessor
{
    public class Processor
    {
        public void Process(CommandLine cl)
        {
            TextureData2D textureData = TextureData2D.FromBitmap(SkiaBitmap.Load(cl.Input));

            Log.Instance.Write($"Compiling Texture {cl.Output} ({textureData.Width}x{textureData.Height}, {textureData.Format.ToString()})");

            if (cl.GenerateMipMaps && textureData.Width != textureData.Height)
            {
                Log.Instance.WriteWarning("Texture not square. Cannot export with mip maps");
                cl.GenerateMipMaps = false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(cl.Output));
            File.WriteAllBytes(cl.Output, Texture2DWriter.Write(cl.GenerateMipMaps ? textureData.GenerateMipMaps() : new List<TextureData2D> { textureData }));
        }
    }
}