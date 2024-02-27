using System.Collections.Generic;
using System.Numerics;
using AngryWasp.Logger;
using SharpGLTF.Schema2;

namespace MeshProcessor.Model
{
    public struct Triangle
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
    }

    public class CollisionMeshFactory
    {
        private Triangle[] triangles;

        public Triangle[] Triangles => triangles;

        public CollisionMeshFactory(ModelRoot srcModel, bool flipWinding)
        {
            var primitiveReaders = new List<CollisionMeshPrimitiveReader>();
            foreach (var mesh in srcModel.LogicalMeshes)
                foreach (var primitive in mesh.Primitives)
                    primitiveReaders.Add(new CollisionMeshPrimitiveReader(primitive));

            if (primitiveReaders.Count > 1)
                Log.Instance.WriteWarning("CollisionMeshFactory only supports glb files with a single primitive. Not all data will be exported");

            var p = primitiveReaders[0];

            triangles = new Triangle[p.Triangles.Length];

            if (flipWinding)
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    var tri = p.Triangles[i];
                    
                    triangles[i] = new Triangle {
                        A = p.Positions[tri.C],
                        B = p.Positions[tri.B],
                        C = p.Positions[tri.A]
                    };
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    var tri = p.Triangles[i];
                    
                    triangles[i] = new Triangle {
                        A = p.Positions[tri.A],
                        B = p.Positions[tri.B],
                        C = p.Positions[tri.C]
                    };
                }
            }
        }
    }
}
