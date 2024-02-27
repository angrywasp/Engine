using System.Collections.Generic;
using AngryWasp.Logger;
using MeshProcessor.Model;
using SharpGLTF.Schema2;

namespace MeshProcessor.Materials
{
    public class MaterialFactory
    {
        private static readonly HashSet<string> supportedChannels = new HashSet<string>
        {
            "BaseColor",
            "Normal",
            "MetallicRoughness",
            "Emissive",
        };

        private Dictionary<SharpGLTF.Schema2.Material, int> materialMapping = new Dictionary<SharpGLTF.Schema2.Material, int>();
        private Dictionary<int, TextureTemplate> textures = new Dictionary<int, TextureTemplate>();
        private Dictionary<int, MaterialTemplate> materials = new Dictionary<int, MaterialTemplate>();

        public IReadOnlyDictionary<int, TextureTemplate> Textures => textures;
        public IReadOnlyDictionary<int, MaterialTemplate> Materials => materials;

        public MaterialFactory(ModelRoot srcModel)
        {
            foreach (var mesh in srcModel.LogicalMeshes)
                foreach (var primitive in mesh.Primitives)
                    UseMaterial(primitive.Material);
        }

        private int UseTexture(int index, TextureTemplate texture)
        {
            if (texture == null)
                return -1;

            if (textures.ContainsKey(index))
            {
                textures[index].AddTypeFlag(texture.Type);
                return index;
            }

            textures.Add(index, texture);
            return index;
        }

        public int UseMaterial(SharpGLTF.Schema2.Material srcMaterial)
        {
            if (srcMaterial == null)
                return -1;

            if (materialMapping.TryGetValue(srcMaterial, out int index))
                return index;

            var dstMaterial = Convert(srcMaterial);

            materials.Add(srcMaterial.LogicalIndex, dstMaterial);
            materialMapping[srcMaterial] = srcMaterial.LogicalIndex;

            return index;
        }

        private MaterialTemplate Convert(SharpGLTF.Schema2.Material srcMaterial)
        {
            if (srcMaterial.FindChannel("MetallicRoughness") == null)
                Log.Instance.WriteFatal("Only the metallic/roughness pipeline is supported");

            var dstMaterial = new MaterialTemplate(srcMaterial.Name);

            dstMaterial.DoubleSided = srcMaterial.DoubleSided;
            //todo: support AlphaCutoff
            dstMaterial.AlphaCutoff = srcMaterial.AlphaCutoff;

            switch (srcMaterial.Alpha)
            {
                case SharpGLTF.Schema2.AlphaMode.OPAQUE:
                    dstMaterial.Mode = MaterialBlendMode.Opaque;
                    break;
                case SharpGLTF.Schema2.AlphaMode.MASK:
                    dstMaterial.Mode = MaterialBlendMode.Mask;
                    break;
                case SharpGLTF.Schema2.AlphaMode.BLEND:
                    dstMaterial.Mode = MaterialBlendMode.Blend;
                    Log.Instance.WriteFatal("AlphaMode.Blend is not supported at this time");
                    break;
            }

            foreach (var srcChannel in srcMaterial.Channels)
            {
                if (!supportedChannels.Contains(srcChannel.Key))
                    continue;

                var dstChannel = dstMaterial.UseChannel(srcChannel.Key);
                if (srcChannel.Texture != null)
                {
                    var imgData = srcChannel.Texture.PrimaryImage.Content.Content.ToArray();
                    dstChannel.TextureIndex = UseTexture(srcChannel.Texture.LogicalIndex, new TextureTemplate(srcChannel.Texture.Name, imgData, MapChannelToTextureType(srcChannel.Key)));
                }
            }

            return dstMaterial;
        }

        private Texture_Type MapChannelToTextureType(string channel)
        {
            switch (channel)
            {
                case "BaseColor": return Texture_Type.Albedo;
                case "Normal": return Texture_Type.Normal;
                case "MetallicRoughness": return Texture_Type.PBR;
                case "Emissive": return Texture_Type.Emissive;
                default: throw new System.Exception($"Invalid channel {channel}");
            }
        }
    }
}
