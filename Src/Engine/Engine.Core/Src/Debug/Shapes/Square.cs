using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugSquare : IDebugShape
    {
        private float size;
        public float Size
        {
            get => size;
            set
            {
                if (size != value)
                    ShapeGenerator.UpdateSquare(value, ref vertices);

                size = value;
            }
        }

        public Vector3 Position { get; set; }

        protected Matrix4x4 worldMatrix;
        protected VertexPositionColor[] vertices;
        protected short[] indices;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugSquare(Vector3 position, int size, Color color)
        {
            Position = position;
            (vertices, indices) = ShapeGenerator.Square(size, color);
        }

        public void DeformHeights(float[] cornerY)
        {
            for (int i = 0; i < 4; i++)
                vertices[i].Position.Y = cornerY[i];
        }

        public void Update()
        {
            worldMatrix = Matrix4x4.CreateTranslation(Position);
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 
                0, vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}
