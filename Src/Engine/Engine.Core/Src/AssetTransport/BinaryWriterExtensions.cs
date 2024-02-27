using System.IO;
using System.Numerics;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Engine.AssetTransport
{
    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter w, VertexPositionTexture v)
        {
            w.Write(v.Position);
            w.Write(v.TexCoord);
        }

        public static void Write(this BinaryWriter w, VertexNormalTangentBinormal v)
        {
            w.Write(v.Normal);
            w.Write(v.Tangent);
            w.Write(v.Binormal);
        }

        public static void Write(this BinaryWriter w, VertexSkinWeight v)
        {
            w.Write(v.Indices);
            w.Write(v.Weights);
        }

        public static void Write(this BinaryWriter w, Vector2 v)
        {
            w.Write(v.X);
            w.Write(v.Y);
        }

        public static void Write(this BinaryWriter w, Vector3 v)
        {
            w.Write(v.X);
            w.Write(v.Y);
            w.Write(v.Z);
        }

        public static void Write(this BinaryWriter w, Vector4 v)
        {
            w.Write(v.X);
            w.Write(v.Y);
            w.Write(v.Z);
            w.Write(v.W);
        }

        public static void Write(this BinaryWriter w, Quaternion v)
        {
            w.Write(v.X);
            w.Write(v.Y);
            w.Write(v.Z);
            w.Write(v.W);
        }

        public static void Write(this BinaryWriter w, BoundingBox b)
        {
            w.Write(b.Min.X);
            w.Write(b.Min.Y);
            w.Write(b.Min.Z);

            w.Write(b.Max.X);
            w.Write(b.Max.Y);
            w.Write(b.Max.Z);
        }

        public static void Write(this BinaryWriter w, Matrix4x4 m)
        {
            w.Write(m.M11);
            w.Write(m.M12);
            w.Write(m.M13);
            w.Write(m.M14);

            w.Write(m.M21);
            w.Write(m.M22);
            w.Write(m.M23);
            w.Write(m.M24);

            w.Write(m.M31);
            w.Write(m.M32);
            w.Write(m.M33);
            w.Write(m.M34);

            w.Write(m.M41);
            w.Write(m.M42);
            w.Write(m.M43);
            w.Write(m.M44);
        }
    }
}
