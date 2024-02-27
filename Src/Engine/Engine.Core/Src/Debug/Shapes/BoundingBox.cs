using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugBoundingBox : IDebugShape
    {
        private BoundingBox box;
        private VertexPositionColor[] vertices;
        private short[] indices;

        public Matrix4x4 WorldMatrix => Matrix4x4.Identity;

        public DebugBoundingBox(BoundingBox boundingBox, Color color)
        {
            this.box = boundingBox;
            (vertices, indices) = ShapeGenerator.BoundingBox(boundingBox, color);
        }

        public void Update(BoundingBox box)
        {
            if (box == this.box)
                return;

            var corners = box.GetCorners();
            for (int i = 0; i < corners.Length; i++)
                vertices[i].Position = corners[i];

            this.box = box;
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 
                0, vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}
