using System.IO;
using System.Numerics;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Engine.AssetTransport
{
    public static class BinaryReaderExtensions
    {
        public static VertexPositionTexture ReadPositionVertex(this BinaryReader r)
        {
            return new VertexPositionTexture(
                r.ReadVector3(),
                r.ReadVector2()
            );
        }

        public static VertexNormalTangentBinormal ReadNormalVertex(this BinaryReader r)
        {
            return new VertexNormalTangentBinormal(
                r.ReadVector3(),
                r.ReadVector3(),
                r.ReadVector3()
            );
        }

        public static VertexSkinWeight ReadSkinVertex(this BinaryReader r)
        {
            return new VertexSkinWeight(
                r.ReadVector4(),
                r.ReadVector4()
            );
        }

        public static Vector2 ReadVector2(this BinaryReader r) => new Vector2(r.ReadSingle(), r.ReadSingle());

        public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        public static Vector4 ReadVector4(this BinaryReader r) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        public static Quaternion ReadQuaternion(this BinaryReader r) => new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        public static BoundingBox ReadBoundingBox(this BinaryReader r)
        {
            var min = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            var max = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

            return new BoundingBox(min, max);
        }

        public static Matrix4x4 ReadMatrix(this BinaryReader r)
        {
            return new Matrix4x4 (
                r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()
            );
        }
    }
}
