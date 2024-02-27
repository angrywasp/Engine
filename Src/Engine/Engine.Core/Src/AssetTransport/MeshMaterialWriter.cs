using System.IO;
using System.Numerics;
using Engine.Graphics.Materials;

namespace Engine.AssetTransport
{
    public static class MeshMaterialWriter
    {
        public static byte[] Write(MeshMaterial m)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(m.AlbedoMap);
                bw.Write(m.NormalMap);
                bw.Write(m.PbrMap);
                bw.Write(m.EmissiveMap);
                bw.Write(m.TextureScale.X);
                bw.Write(m.TextureScale.Y);
                bw.Write(m.DoubleSided);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        public static byte[] Write(string albedo, string normal, string pbr, string emissive, Vector2 texScale, bool doubleSided)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write(albedo);
                bw.Write(normal);
                bw.Write(pbr);
                bw.Write(emissive);
                bw.Write(texScale.X);
                bw.Write(texScale.Y);
                bw.Write(doubleSided);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }
    }
}
