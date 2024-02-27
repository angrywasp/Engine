using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Engine.Content.Model.Template;
using Engine.Graphics.Vertices;

namespace Engine.AssetTransport
{
    public class MeshWriter
    {
        public static byte[] Write(MeshTemplate mesh)
        {
            using (var bw = new BinaryWriter(new MemoryStream()))
            {
                bw.Write((byte)mesh.Type);

                if (mesh.Type == Mesh_Type.Skinned)
                    SkeletonWriter.Write(bw, mesh.Skeleton, mesh.SubMeshes.Select(x => x.Skin).ToArray());
                
                var indices = MemoryMarshal.AsBytes<int>(mesh.Indices);
                bw.Write(indices.Length);
                bw.Write(indices);

                var positions = MemoryMarshal.AsBytes<VertexPositionTexture>(mesh.Positions);
                bw.Write(positions.Length);
                bw.Write(positions);
                
                var normals = MemoryMarshal.AsBytes<VertexNormalTangentBinormal>(mesh.Normals);
                bw.Write(normals.Length);
                bw.Write(normals);

                if (mesh.Type == Mesh_Type.Skinned)
                {
                    var skinWeights = MemoryMarshal.AsBytes<VertexSkinWeight>(mesh.SkinWeights);
                    bw.Write(skinWeights.Length);
                    bw.Write(skinWeights);
                }

                bw.Write(mesh.BoundingBox);
                bw.Write(mesh.SubMeshes.Length);

                foreach (var s in mesh.SubMeshes)
                    WriteSubMesh(bw, s);

                return ((MemoryStream)bw.BaseStream).ToArray();
            }
        }

        private static void WriteSubMesh(BinaryWriter w, SubMeshTemplate s)
        {
            w.Write(s.BaseVertex);
            w.Write(s.NumVertices);
            w.Write(s.StartIndex);
            w.Write(s.IndexCount);

            w.Write(s.BoundingBox);

            w.Write(string.IsNullOrEmpty(s.Material) ? string.Empty : s.Material);
        }
    }
}
