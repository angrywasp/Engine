using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Engine.Content
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexParticle : IVertexType
    {
        public Short2 Corner;
        public Vector3 Position;
        public Vector3 Velocity;
        public Color Random;
        public float Time;

        public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public const int VERTEX_SIZE = 36;

        static VertexParticle()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Short2, VertexElementUsage.Position, 0),
                new VertexElement(4, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(28, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Corner.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Velocity.GetHashCode();
                hashCode = (hashCode * 397) ^ Random.GetHashCode();
                hashCode = (hashCode * 397) ^ Time.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"<Corner: {this.Corner} Velocity: {this.Velocity} Time: {this.Time}>";
    }
}
