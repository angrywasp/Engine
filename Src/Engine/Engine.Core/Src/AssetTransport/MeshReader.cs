using System.IO;
using System.Runtime.InteropServices;
using Engine.Content.Model.Template;
using Engine.Graphics.Vertices;

namespace Engine.AssetTransport
{
    public static class MeshReader
    {
        public static MeshTemplate Read(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                var type = (Mesh_Type)br.ReadByte();

                SkeletonTemplate skeleton = null;
                SkinTemplate[] skin = null;
                if (type == Mesh_Type.Skinned)
                    (skeleton, skin) = SkeletonReader.Read(br);
                
                var indicesLength = br.ReadInt32();
                var indicesData = br.ReadBytes(indicesLength);
                var indices = MemoryMarshal.Cast<byte, int>(indicesData).ToArray();

                var positionsLength = br.ReadInt32();
                var positionData = br.ReadBytes(positionsLength);
                var positions = MemoryMarshal.Cast<byte, VertexPositionTexture>(positionData).ToArray();

                var normalsLength = br.ReadInt32();
                var normalData = br.ReadBytes(normalsLength);
                var normals = MemoryMarshal.Cast<byte, VertexNormalTangentBinormal>(normalData).ToArray();

                VertexSkinWeight[] skinWeights = null;

                if (type == Mesh_Type.Skinned)
                {
                    var skinWeightsLength = br.ReadInt32();
                    var skinWeightsData = br.ReadBytes(skinWeightsLength);
                    skinWeights = MemoryMarshal.Cast<byte, VertexSkinWeight>(skinWeightsData).ToArray();
                }

                var boundingBox = br.ReadBoundingBox();

                var subMeshes = new SubMeshTemplate[br.ReadInt32()];
                for (int i = 0; i < subMeshes.Length; i++)
                    subMeshes[i] = new SubMeshTemplate(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadBoundingBox(), skin == null ? null : skin[i], br.ReadString());

                return new MeshTemplate(indices, positions, normals, skinWeights, boundingBox, subMeshes, skeleton);
            }
        }
    }
}
