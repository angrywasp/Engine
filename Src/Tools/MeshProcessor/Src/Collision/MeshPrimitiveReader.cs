using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AngryWasp.Logger;
using Engine.Graphics.Vertices;
using SharpGLTF.Schema2;

namespace MeshProcessor.Model
{
    public sealed class CollisionMeshPrimitiveReader
    {
        private readonly (int A, int B, int C)[] triangles;
        private readonly Vector3[] positions;

        public (int A, int B, int C)[] Triangles => triangles;
        public Vector3[] Positions => positions;

        public CollisionMeshPrimitiveReader(MeshPrimitive srcPrim)
        {
            positions = srcPrim.GetVertexAccessor("POSITION")?.AsVector3Array().ToArray();
            triangles = srcPrim.GetTriangleIndices().ToArray();
        }
    }
}
