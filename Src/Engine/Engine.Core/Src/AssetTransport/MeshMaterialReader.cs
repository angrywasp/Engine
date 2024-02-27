using System.IO;
using System.Numerics;

namespace Engine.AssetTransport
{
    public static class MeshMaterialReader
    {
        public static (string Albedo, string Normal, string Pbr, string Emissive, Vector2 TextureScale, bool DoubleSided) Read(byte[] data)
        {
            using (var fs = new MemoryStream(data))
            {
                using (var br = new BinaryReader(fs))
                {
                    var albedo = br.ReadString();
                    var normal = br.ReadString();
                    var pbr = br.ReadString();
                    var emissive = br.ReadString();
                    var texScale = new Vector2(br.ReadSingle(), br.ReadSingle());
                    var doubleSided = br.ReadBoolean();

                    return (albedo, normal, pbr, emissive, texScale, doubleSided);
                }
            }
        }
    }
}
