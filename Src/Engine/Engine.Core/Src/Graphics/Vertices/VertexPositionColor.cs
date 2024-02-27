using System.Runtime.InteropServices;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Engine.Graphics.Vertices
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColor : IVertexType
	{
		public Vector3 Position;
		public Color Color;

		public static readonly VertexDeclaration VertexDeclaration;
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

		public VertexPositionColor(Vector3 position, Color color)
		{
			this.Position = position;
			Color = color;
		}

        static VertexPositionColor()
		{
			VertexElement[] elements = new VertexElement[]
            {
                new VertexElement (0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement (12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            };

			VertexDeclaration = new VertexDeclaration (elements);
		}


	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return (Position.GetHashCode() * 397) ^ Color.GetHashCode();
	        }
	    }

	    public override string ToString() => $"<Position: {this.Position}, Color: {this.Color}>";
	}
}
