using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AngryWasp.Cryptography;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.AssetTransport;
using Engine.Bitmap.Data;
using Engine.Graphics.Materials;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;

namespace MeshProcessor.Materials
{
    public class MaterialProcessor
    {
        private Dictionary<int, string> materials = new Dictionary<int, string>();

        public Dictionary<int, string> Materials => materials;

        public MaterialProcessor(ModelRoot model, CommandLine cl)
        {
            var materialFactory = new MaterialFactory(model);

            var texturePaths = new Dictionary<int, string>();

            if (!string.IsNullOrEmpty(cl.TextureOutput))
            {
                foreach (var t in materialFactory.Textures)
                    texturePaths.Add(t.Key, ExportTexture(t.Value, cl));
            }

            foreach (var mat in materialFactory.Materials)
            {
                if (string.IsNullOrEmpty(cl.MaterialOutput) || string.IsNullOrEmpty(cl.TextureOutput))
                {
                    materials.Add(mat.Key, "Engine/Materials/Default.material");
                    continue;
                }

                string filePath = Path.Combine(cl.MaterialOutput, $"{Path.GetFileNameWithoutExtension(mat.Value.Name)}.material").NormalizeFilePath();
                filePath = StringHelper.RemoveWhitespace(filePath);
                filePath = StringHelper.ReplaceAll(filePath, new char[] {',', '[', ']'}, new char[] {'.', '(', ')'});

                materials.Add(mat.Key, filePath);

                if (File.Exists(EngineFolders.ContentPathVirtualToReal(filePath)))
                    continue;

                int albedoIndex = mat.Value.FindChannel("BaseColor").TextureIndex;
                int normalIndex = mat.Value.FindChannel("Normal").TextureIndex;
                int pbrIndex = mat.Value.FindChannel("MetallicRoughness").TextureIndex;
                int emissiveIndex = mat.Value.FindChannel("Emissive").TextureIndex;

                MeshMaterial material = new MeshMaterial();
                
                if (albedoIndex != -1)
                    material.AlbedoMap = texturePaths[albedoIndex];

                if (normalIndex != -1)
                    material.NormalMap = texturePaths[normalIndex];

                if (pbrIndex != -1)
                    material.PbrMap = texturePaths[pbrIndex];

                if (emissiveIndex != -1)
                    material.EmissiveMap = texturePaths[emissiveIndex];

                material.DoubleSided = mat.Value.DoubleSided;

                File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(filePath), MeshMaterialWriter.Write(material));
            }
        }

        private string ExportTexture(TextureTemplate texture, CommandLine cl)
        {
            if (texture.Data == null)
            {
                switch (texture.Type)
                {
                    case Texture_Type.Normal:
                        return "Engine/Textures/Default_normal.texture";
                    case Texture_Type.PBR:
                        return "Engine/Textures/Default_pbr.texture";
                    case Texture_Type.Emissive:
                        return "Engine/Textures/Default_emissive.texture";
                    default:
                        return "Engine/Textures/Default_albedo.texture";
                }
            }

            string textureName = "." + Base58.Encode(Keccak.Hash128(texture.Data.Interleave()));

            string filePath = Path.Combine(cl.TextureOutput, $"{textureName}_{texture.Type.ToString().ToLower()}.texture").NormalizeFilePath();
            filePath = StringHelper.RemoveWhitespace(filePath);
            filePath = StringHelper.ReplaceAll(filePath, new char[] {',', '-', '[', ']'}, new char[] {'.', '.', '(', ')'});

            if (File.Exists(EngineFolders.ContentPathVirtualToReal(filePath)))
                return filePath;

            Log.Instance.Write($"Exporting mesh texture {filePath}");

            TextureData2D exportData;
            
            if (texture.Type.HasFlag(Texture_Type.PBR))
            {
                var pbr = new TextureData(texture.Data.Width, texture.Data.Height, SurfaceFormat.Rgba);
                pbr.CopyChannel(TextureData_Channel.Red, texture.Data.Blue);
                pbr.CopyChannel(TextureData_Channel.Green, texture.Data.Red);
                pbr.CopyChannel(TextureData_Channel.Blue, texture.Data.Green);
                pbr.Fill(TextureData_Channel.Alpha, 255);
                exportData = pbr.ToTextureData2D();
            }
            else if (texture.Type.HasFlag(Texture_Type.Normal) && cl.InvertNormals)
            {
                texture.Data.InvertChannel(TextureData_Channel.Green);
                exportData = texture.Data.ToTextureData2D();
            }
            else
                exportData = texture.Data.ToTextureData2D();

            File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(filePath), Texture2DWriter.Write(cl.GenerateMipMaps ? exportData.GenerateMipMaps() : new List<TextureData2D> { exportData }));

            return filePath;
        }
    }
}
