using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexNormalTangentBinormal : IVertexType
    {
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexNormalTangentBinormal(Vector3 normal)
        {
            Normal = normal;
            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        public VertexNormalTangentBinormal(Vector3 normal, Vector4 tangent)
        {
            Normal = normal;
            Tangent = new Vector3(tangent.X, tangent.Y, tangent.Z);
            Binormal = Vector3.Cross(Tangent, Normal) * tangent.W;
        }

        public VertexNormalTangentBinormal(Vector3 normal, Vector3 tangent, Vector3 binormal)
        {
            Normal = normal;
            Tangent = tangent;
            Binormal = binormal;
        }

        public void SetTangent(Vector4 tangent)
        {
            Tangent = new Vector3(tangent.X, tangent.Y, tangent.Z);
            Binormal = Vector3.Cross(Tangent, Normal) * tangent.W;
        }

        static VertexNormalTangentBinormal()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Tangent.GetHashCode();
                hashCode = (hashCode * 397) ^ Binormal.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"<Normal: {this.Normal}, Tangent: {this.Tangent}, Binormal: {this.Binormal}>";
    }
}
