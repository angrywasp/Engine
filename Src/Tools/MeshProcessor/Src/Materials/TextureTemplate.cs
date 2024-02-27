using System;
using AngryWasp.Logger;
using Engine.Bitmap;
using Engine.Bitmap.Data;
using SharpGLTF.Memory;

namespace MeshProcessor.Materials
{
    [Flags]
    public enum Texture_Type
    {
        None = 0,
        Albedo = 1,
        Normal = 2,
        PBR = 4,
        Emissive = 8
    }

    public class TextureTemplate
    {
        private string name;
        private TextureData data;
        private Texture_Type type = Texture_Type.None;

        public string Name => name;
        public TextureData Data => data;
        public Texture_Type Type => type;

        public TextureTemplate(string name, MemoryImage img, Texture_Type type)
        {
            this.name = name;
            this.type = type;

            if (img.IsEmpty)
            {
                this.data = null;
            }
            else
            {
                var bmp = SkiaBitmap.Load(img.Content.ToArray());
                if (bmp == null)
                    Log.Instance.WriteFatal($"Unsupported file type {name}. Mime type {img.MimeType} is not supported");
                    
                this.data = TextureData.FromBitmap(bmp);
            }
        }

        public void AddTypeFlag(Texture_Type type)
        {
            if (this.type == type)
                return;

            if (this.type == Texture_Type.PBR && type != Texture_Type.PBR)
                Log.Instance.WriteWarning($"Texture is assigned to a channel other than the PBR channel.\r\nThis could result in incorrect texture exporting");

            this.type |= type;
        }

        public void RemoveTypeFlag(Texture_Type type) => this.type &= ~type;
    }
}
