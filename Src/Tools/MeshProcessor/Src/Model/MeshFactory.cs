using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Content.Model.Template;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using SharpGLTF.Schema2;

namespace MeshProcessor.Model
{
    public class MeshFactory
    {
        private MeshTemplate mesh;

        public MeshTemplate Mesh => mesh;

        public MeshFactory(ModelRoot srcModel, SkeletonTemplate skeletonTemplate, SkinTemplate[] skinTemplates, Dictionary<int, string> materials, bool flipWinding, bool recalculateNormals)
        {
            var sortedByMaterial = MeshPrimitiveReader.SortMeshesByMaterial(srcModel);

            if (skinTemplates != null && sortedByMaterial.Count != skinTemplates.Length)
                Log.Instance.WriteFatal("Submesh/skin count mismatch");

            int baseVertex = 0;
            int startIndex = 0;

            var meshPositions = new List<VertexPositionTexture>();
            var meshNormals = new List<VertexNormalTangentBinormal>();
            var meshSkins = new List<VertexSkinWeight>();
            var meshIndices = new List<int>();
            var meshSubMeshes = new List<SubMeshTemplate>();
            var boundingBox = new BoundingBox();
            var triangles = new List<(int A, int B, int C)>();

            bool needRecalculateNormals = false;
            bool needRecalculateTangents = false;

            foreach (var m in sortedByMaterial)
            {
                List<MeshPrimitiveReader> meshPrimitiveReaders = new List<MeshPrimitiveReader>();

                foreach (var p in m.Value)
                    meshPrimitiveReaders.Add(new MeshPrimitiveReader(p));

                var subMeshPositions = new List<VertexPositionTexture>();
                var subMeshNormals = new List<VertexNormalTangentBinormal>();
                var subMeshSkins = new List<VertexSkinWeight>();
                var subMeshIndices = new List<int>();

                foreach (var mp in meshPrimitiveReaders)
                {
                    var verts = mp.ToMeshVertices(out bool hasTangents, out bool hasSkin);

                    if (!hasTangents)
                        needRecalculateTangents = true;

                    subMeshPositions.AddRange(verts.Positions);
                    subMeshNormals.AddRange(verts.Normals);

                    if (hasSkin)
                        subMeshSkins.AddRange(verts.SkinWeights);

                    if (flipWinding)
                    {
                        foreach (var ti in mp.Triangles)
                        {
                            subMeshIndices.Add(ti.A);
                            subMeshIndices.Add(ti.B);
                            subMeshIndices.Add(ti.C);
                        }
                    }
                    else
                    {
                        foreach (var ti in mp.Triangles)
                        {
                            subMeshIndices.Add(ti.C);
                            subMeshIndices.Add(ti.B);
                            subMeshIndices.Add(ti.A);
                        }
                    }

                    triangles.AddRange(mp.Triangles);
                }

                meshPositions.AddRange(subMeshPositions);
                meshNormals.AddRange(subMeshNormals);
                meshSkins.AddRange(subMeshSkins);

                meshIndices.AddRange(subMeshIndices);

                var subMeshBoundingBox = BoundingBox.CreateFromPoints(subMeshPositions.Select(x => x.Position));
                boundingBox = BoundingBox.CreateMerged(boundingBox, subMeshBoundingBox);

                string material = m.Key == -1 || !materials.ContainsKey(m.Key) ? null : materials[m.Key];

                meshSubMeshes.Add(new SubMeshTemplate(baseVertex, subMeshPositions.Count, startIndex, subMeshIndices.Count, subMeshBoundingBox, skinTemplates == null ? null : skinTemplates[m.Key], material));

                baseVertex += subMeshPositions.Count;
                startIndex += subMeshIndices.Count;
            }

            if (flipWinding)
            {
                for (int i = 0; i < meshNormals.Count; i++)
                    meshNormals[i] = new VertexNormalTangentBinormal(-meshNormals[i].Normal);

                needRecalculateTangents = true;
            }

            if (!recalculateNormals)
                foreach (var n in meshNormals)
                {
                    if (!n.Normal.IsValid())
                    {
                        needRecalculateNormals = true;
                        break; //can break early cause recalulating normals will trigger a tangent recalculation
                    }

                    if (!n.Binormal.IsValid() || !n.Tangent.IsValid())
                        needRecalculateTangents = true;
                }

            var positions = meshPositions.ToArray();
            var normals = meshNormals.ToArray();
            var skins = meshSkins.ToArray();
            var indices = meshIndices.ToArray();
            var tris = triangles.ToArray();

            if (needRecalculateNormals || recalculateNormals)
            {
                Log.Instance.WriteWarning("Recalculating normals");
                normals = CalculateNormals(positions, tris);
                needRecalculateTangents = true;
            }

            if (needRecalculateTangents)
            {
                Log.Instance.WriteWarning("Recalculating tangents");
                var tangents = CalculateTangents(positions, normals, tris);
                for (int i = 0; i < normals.Length; i++)
                {
                    if (!tangents[i].IsValid())
                        Log.Instance.WriteFatal("Invalid tangent data. Check for broken/missing texture coordinates");

                    normals[i].SetTangent(tangents[i]);
                }
            }

            mesh = new MeshTemplate(indices, positions, normals, skins, boundingBox, meshSubMeshes.ToArray(), skeletonTemplate);
        }

        private static VertexNormalTangentBinormal[] CalculateNormals(Span<VertexPositionTexture> positions, Span<(int A, int B, int C)> triangles)
        {
            VertexNormalTangentBinormal[] normalVertices = new VertexNormalTangentBinormal[positions.Length];

            //for each triangle, we need to calculate the face normal
            var faceNormals = new Vector3[triangles.Length];
            var normals = new Vector3[positions.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                var tri = triangles[i];
                Vector3 side1 = positions[tri.B].Position - positions[tri.A].Position;
                Vector3 side2 = positions[tri.C].Position - positions[tri.A].Position;
                faceNormals[i] = Vector3.Normalize(Vector3.Cross(side1, side2));

                if (!faceNormals[i].IsValid())
                    Log.Instance.WriteFatal("Invalid normal data. Cannot process the mesh.");
            }

            //now accumulate all those face values into a buffer
            for (int i = 0; i < triangles.Length; i++)
            {
                var tri = triangles[i];
                var faceNormal = faceNormals[i];
                normals[tri.A] += faceNormal;
                normals[tri.B] += faceNormal;
                normals[tri.C] += faceNormal;
            }

            //now normalize the results
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
                normalVertices[i] = new VertexNormalTangentBinormal(normals[i]);
            }

            return normalVertices;
        }

        private static Vector4[] CalculateTangents(Span<VertexPositionTexture> positions, Span<VertexNormalTangentBinormal> normals, Span<(int A, int B, int C)> triangles)
        {
            Vector3[] tan1 = new Vector3[positions.Length];
            Vector3[] tan2 = new Vector3[positions.Length];
            Vector4[] tangents = new Vector4[positions.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                var tri = triangles[i];
                Vector3 v1 = positions[tri.A].Position;
                Vector3 v2 = positions[tri.B].Position;
                Vector3 v3 = positions[tri.C].Position;
                Vector2 w1 = positions[tri.A].TexCoord;
                Vector2 w2 = positions[tri.B].TexCoord;
                Vector2 w3 = positions[tri.C].TexCoord;
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
                tan1[tri.A] += sdir;
                tan1[tri.B] += sdir;
                tan1[tri.C] += sdir;
                tan2[tri.A] += tdir;
                tan2[tri.B] += tdir;
                tan2[tri.C] += tdir;
            }

            for (int a = 0; a < tangents.Length; ++a)
            {
                Vector3 n = normals[a].Normal;
                Vector3 t = tan1[a];
                Vector3 tmp = Vector3.Normalize(t - n * Vector3.Dot(n, t));
                tangents[a] = new Vector4(tmp.X, tmp.Y, tmp.Z, (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f ? -1.0f : 1.0f));
            }

            return tangents;
        }
    }
}
