using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using AngryWasp.Logger;
using Engine.AssetTransport;
using Engine.Content.Model.Template;
using Engine.Graphics.Materials;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;

namespace Engine.Content.Model
{
    public static class MeshUtils
    {
        public static Vector4[] CalculateTangents(VertexPositionTexture[] positions, VertexNormalTangentBinormal[] normals, int[] indices)
        {
            if (positions.Length != normals.Length)
                Log.Instance.WriteFatal("Positions.Length != Normals.Length");

            var length = positions.Length;

            Vector3[] tan1 = new Vector3[length];
            Vector3[] tan2 = new Vector3[length];
            Vector4[] tangents = new Vector4[length];

            for (int a = 0; a < indices.Length; a += 3)
            {
                var i1 = indices[a + 0];
                var i2 = indices[a + 1];
                var i3 = indices[a + 2];
                Vector3 v1 = positions[i1].Position;
                Vector3 v2 = positions[i2].Position;
                Vector3 v3 = positions[i3].Position;
                Vector2 w1 = positions[i1].TexCoord;
                Vector2 w2 = positions[i2].TexCoord;
                Vector2 w3 = positions[i3].TexCoord;
                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;
                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;
                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (int a = 0; a < length; ++a)
            {
                Vector3 n = normals[a].Normal;
                Vector3 t = tan1[a];
                Vector3 tmp = Vector3.Normalize(t - n * Vector3.Dot(n, t));
                tangents[a] = new Vector4(tmp.X, tmp.Y, tmp.Z, (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f ? -1.0f : 1.0f));
            }

            return tangents;
        }

        public static T ShallowCopy<T>(MeshMaterial mat) where T : MeshMaterial, new()
        {
            return new T
            {
                AlbedoMap = mat.AlbedoMap,
                NormalMap = mat.NormalMap,
                PbrMap = mat.PbrMap,
                EmissiveMap = mat.EmissiveMap,
                TextureScale = mat.TextureScale,
                DoubleSided = mat.DoubleSided
            };
        }

        public static MeshTemplate Transform(this MeshTemplate mesh, Matrix4x4 transform)
        {
            var transformedPositions = mesh.Positions.Select(x => {
                x.Position = Vector3.Transform(x.Position, transform);
                return x;
            }).ToArray();

            var transformedNormals = mesh.Normals.Select(x => {
                x.Normal = Vector3.TransformNormal(x.Normal, transform);
                x.Tangent = Vector3.TransformNormal(x.Tangent, transform);
                x.Binormal = Vector3.TransformNormal(x.Binormal, transform);
                return x;
            }).ToArray();

            SubMeshTemplate[] subMeshes = new SubMeshTemplate[mesh.SubMeshes.Length];
            
            for (int i = 0; i < subMeshes.Length; i++)
            {
                var s = mesh.SubMeshes[i];
                var bb = BoundingBox.CreateFromPoints(transformedPositions.AsSpan().Slice(s.BaseVertex, s.NumVertices).ToArray().Select(x => x.Position));
                subMeshes[i] = new SubMeshTemplate(s.BaseVertex, s.NumVertices, s.StartIndex, s.IndexCount, bb, null, s.Material);
            }

            var boundingBox = subMeshes[0].BoundingBox;
            for (int i = 0; i < subMeshes.Length; i++)
                boundingBox = BoundingBox.CreateMerged(boundingBox, subMeshes[i].BoundingBox);

            return new MeshTemplate(mesh.Indices, transformedPositions, transformedNormals, null, boundingBox, subMeshes, null);
        }

        private class SubMeshVertexData
        {
            private string material;
            private VertexPositionTexture[] positionVertices;
            private VertexNormalTangentBinormal[] normalVertices;
            private int[] indices;

            public SubMeshVertexData(string material, VertexPositionTexture[] positionVertices, VertexNormalTangentBinormal[] normalVertices, int[] indices)
            {
                this.material = material;
                this.positionVertices = positionVertices;
                this.normalVertices = normalVertices;
                this.indices = indices;
            }
        }

        public static MeshTemplate MergeMeshes(List<(string FullMeshPath, Matrix4x4 Transform)> meshList)
        {
            //Merging meshes involves splitting the mesh list down into submeshes and grouping by their material
            //This splitting also grabs a copy of the vertices for that part of the submesh
            //Then we will merge all the submesh vertex spans for each material into a single vertex span
            //effectively mergine all the submeshes for a particular material into a single submesh
            //then we make the mesh from the submeshes and voila

            var meshes = new MeshTemplate[meshList.Count];
            for (int i = 0; i < meshes.Length; i++)
                meshes[i] = MeshReader.Read(File.ReadAllBytes(meshList[i].FullMeshPath)).Transform(meshList[i].Transform);

            var sorted = new Dictionary<string, List<SubMeshVertexData>>();
            for (int x = 0; x < meshes.Length; x++)
            {
                var mesh = meshes[x];
                for (int y = 0; y < mesh.SubMeshes.Length; y++)
                {
                    var subMesh = mesh.SubMeshes[y];
                    if (!sorted.ContainsKey(subMesh.Material))
                        sorted.Add(subMesh.Material, new List<SubMeshVertexData>());


                    var p = mesh.Positions.AsSpan().Slice(subMesh.BaseVertex, subMesh.NumVertices).ToArray();
                    var n = mesh.Normals.AsSpan().Slice(subMesh.BaseVertex, subMesh.NumVertices).ToArray();
                    var i = mesh.Indices.AsSpan().Slice(subMesh.StartIndex, subMesh.IndexCount).ToArray();

                    sorted[subMesh.Material].Add(new SubMeshVertexData(subMesh.Material, p, n, i));
                }
            }

            throw new NotImplementedException();
        }
    }
}
