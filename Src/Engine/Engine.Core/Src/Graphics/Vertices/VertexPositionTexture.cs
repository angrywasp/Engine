using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoord;

        public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionTexture(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
        
        static VertexPositionTexture()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ TexCoord.GetHashCode();
            }
        }

        public override string ToString() => $"<Position: {this.Position}, TexCoord: {this.TexCoord}>";
    }
}
