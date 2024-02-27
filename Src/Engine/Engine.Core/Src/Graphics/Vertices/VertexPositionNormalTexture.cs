using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texCoord;
        }

        static VertexPositionNormalTexture()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(0x18, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ TexCoord.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"<Position: {this.Position}, Normal: {this.Normal}, TexCoord: {this.TexCoord}>";
    }
}
