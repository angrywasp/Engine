using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngryWasp.Logger;
using Engine.Graphics.Vertices;
using SharpGLTF.Schema2;

namespace MeshProcessor.Model
{
    public sealed class MeshPrimitiveReader
    {
        private readonly (int A, int B, int C)[] triangles;

        private readonly IList<Vector3> positions;
        private readonly IList<Vector3> normals;
        private readonly IList<Vector4> tangents;

        private readonly IList<Vector4> color0;
        private readonly IList<Vector2> texCoord0;

        private readonly IList<Vector4> joints0;
        private readonly IList<Vector4> joints1;

        private readonly IList<Vector4> weights0;
        private readonly IList<Vector4> weights1;

        public (int A, int B, int C)[] Triangles => triangles;

        public MeshPrimitiveReader(MeshPrimitive srcPrim)
        {
            positions = srcPrim.GetVertexAccessor("POSITION")?.AsVector3Array();
            normals = srcPrim.GetVertexAccessor("NORMAL")?.AsVector3Array();
            tangents = srcPrim.GetVertexAccessor("TANGENT")?.AsVector4Array();

            color0 = srcPrim.GetVertexAccessor("COLOR_0")?.AsColorArray();
            texCoord0 = srcPrim.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();

            joints0 = srcPrim.GetVertexAccessor("JOINTS_0")?.AsVector4Array();
            joints1 = srcPrim.GetVertexAccessor("JOINTS_1")?.AsVector4Array();
            weights0 = srcPrim.GetVertexAccessor("WEIGHTS_0")?.AsVector4Array();
            weights1 = srcPrim.GetVertexAccessor("WEIGHTS_1")?.AsVector4Array();

            if (joints0 == null || weights0 == null) { joints0 = joints1 = weights0 = weights1 = null; }
            if (joints1 == null || weights1 == null) { joints1 = weights1 = null; }

            if (weights0 != null)
            {
                weights0 = weights0.ToArray();

                for (int i = 0; i < weights0.Count; ++i)
                {
                    var r = Vector4.Dot(weights0[i], Vector4.One);
                    weights0[i] /= r;
                }
            }

            triangles = srcPrim.GetTriangleIndices().ToArray();
        }

        public Vector3 GetPosition(int idx) => positions[idx];

        public Vector3 GetNormal(int idx) => normals[idx];

        public Vector4 GetTangent(int idx) => tangents[idx];

        public Vector2 GetTextureCoord(int idx, int set)
        {
            if (set == 0 && texCoord0 != null)
                return texCoord0[idx];

            return Vector2.Zero;
        }

        public Vector4 GetColor(int idx, int set)
        {
            if (set == 0 && color0 != null)
                return color0[idx];

            return Vector4.One;
        }

        public Vector4 GetIndices(int idx)
        {
            if (joints0 != null)
                return joints0[idx];

            return Vector4.Zero;
        }

        public Vector4 GetWeights(int idx)
        {
            if (weights0 != null)
                return weights0[idx];

            return Vector4.UnitX;
        }

        public (VertexPositionTexture[] Positions, VertexNormalTangentBinormal[] Normals, VertexSkinWeight[] SkinWeights) ToMeshVertices(out bool hasTangents, out bool hasSkin)
        {
            if (positions.Count != normals.Count)
                Log.Instance.WriteFatal("Vertex array length mismatch");

            if (joints1 != null || weights1 != null)
                Log.Instance.WriteFatal("skin data in channel 1 is not supported. Channel 0 only");   

            hasSkin = joints0 != null && weights0 != null;

            if (hasSkin)
            {
                if (joints0.Count != weights0.Count || joints0.Count != positions.Count)
                {
                    Log.Instance.WriteWarning("Incomplete skin data found. Exporting without skin data");
                    hasSkin = false;
                }
            }

            var p = new VertexPositionTexture[positions.Count];
            var n = new VertexNormalTangentBinormal[normals.Count];
            var s = hasSkin ? new VertexSkinWeight[joints0.Count] : null;

            if (tangents != null)
            {
                hasTangents = true;
                for (int i = 0; i < positions.Count; ++i)
                {
                    p[i] = new VertexPositionTexture(GetPosition(i), GetTextureCoord(i, 0));
                    n[i] = new VertexNormalTangentBinormal(GetNormal(i), GetTangent(i));
                }
            }
            else
            {
                hasTangents = false;
                for (int i = 0; i < positions.Count; ++i)
                {
                    p[i] = new VertexPositionTexture(GetPosition(i), GetTextureCoord(i, 0));
                    n[i] = new VertexNormalTangentBinormal(GetNormal(i));
                }
            }

            if (hasSkin)
                for (int i = 0; i < positions.Count; ++i)
                    s[i] = new VertexSkinWeight(GetIndices(i), GetWeights(i));

            return (p, n, s);
        }

        public static Dictionary<int, List<MeshPrimitive>> SortMeshesByMaterial(ModelRoot model)
        {
            var ret = new Dictionary<int, List<MeshPrimitive>>();
            foreach (var mesh in model.LogicalMeshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    int idx = primitive.Material == null ? -1 : primitive.Material.LogicalIndex;

                    if (!ret.ContainsKey(idx))
                        ret.Add(idx, new List<MeshPrimitive>());

                    ret[idx].Add(primitive);
                }
            }

            return ret;
        }
    }
}
