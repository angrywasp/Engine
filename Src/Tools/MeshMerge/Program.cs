using AngryWasp.Logger;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using AngryWasp.Cli.Args;
using System.IO;
using Engine.Graphics.Vertices;
using Engine.AssetTransport;
using Engine.Content.Model.Template;

namespace MeshMerge
{
    public class Program
    {
        private static string inNameA;
        private static string inNameB;
        private static string output;

        private static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);

            inNameA = args.Pop().Value;
            inNameB = args.Pop().Value;
            output = args.Pop().Value;

            Log.Instance.Write("Merging Meshes");

            var m1 = MeshReader.Read(File.ReadAllBytes(inNameA));
            var m2 = MeshReader.Read(File.ReadAllBytes(inNameB));

            if (m1.SkinWeights != null || m2.SkinWeights != null)
                Log.Instance.WriteFatal("MeshMerge does not support skinned meshes");

            var indices = new List<int>();
            var positions = new List<VertexPositionTexture>();
            var normals = new List<VertexNormalTangentBinormal>();
            var subMeshes = new List<SubMeshTemplate>();

            positions.AddRange(m1.Positions);
            normals.AddRange(m1.Normals);
            indices.AddRange(m1.Indices);

            positions.AddRange(m2.Positions);
            normals.AddRange(m2.Normals);
            indices.AddRange(m2.Indices);

            var boundingBox = new BoundingBox();

            for (int i = 0; i < m1.SubMeshes.Length; i++)
            {
                boundingBox = BoundingBox.CreateMerged(boundingBox, m1.SubMeshes[i].BoundingBox);
                subMeshes.Add(m1.SubMeshes[i]);
            }

            for (int i = 0; i < m2.SubMeshes.Length; i++)
            {
                var s = m2.SubMeshes[i];
                boundingBox = BoundingBox.CreateMerged(boundingBox, m2.SubMeshes[i].BoundingBox);
                subMeshes.Add(new SubMeshTemplate(s.BaseVertex + m1.Positions.Length, s.NumVertices, s.StartIndex + m1.Indices.Length, s.IndexCount, s.BoundingBox, null, s.Material));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var mesh = new MeshTemplate(indices.ToArray(), positions.ToArray(), normals.ToArray(), null, boundingBox, subMeshes.ToArray(), null);
            File.WriteAllBytes(output, MeshWriter.Write(mesh));
        }
    }
}