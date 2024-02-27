using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexSkinWeight : IVertexType
    {
        public Vector4 Indices;
        public Vector4 Weights;

        public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexSkinWeight(Vector4 indices, Vector4 weights)
        {
            Indices = indices;
            Weights = weights;
        }
        
        static VertexSkinWeight()
        {
            VertexElement[] elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            };

            VertexDeclaration = new VertexDeclaration(elements);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Indices.GetHashCode() * 397) ^ Weights.GetHashCode();
            }
        }

        public override string ToString() => $"<Indices: {this.Indices}, Weights: {this.Weights}>";
    }
}
