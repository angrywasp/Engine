using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugCircle : IDebugShape
    {
        private float radius;
        private int resolution;

        public float Radius
        {
            get => radius;
            set
            {
                if (radius != value)
                    ShapeGenerator.UpdateCircle(value, resolution, ref vertices);

                radius = value;
            }
        }

        public int Resolution => resolution;

        public Vector3 Position { get; set; }

        protected Matrix4x4 worldMatrix;
        protected VertexPositionColor[] vertices;
        protected short[] indices;

        public VertexPositionColor[] Vertices => vertices;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugCircle(Vector3 position, float radius, int resolution, Color color)
        {
            this.radius = radius;
            this.resolution = resolution;
            Position = position;
            (vertices, indices) = ShapeGenerator.Circle(radius, resolution, color);
        }

        public void DeformHeights(float[] heights)
        {
            for (int i = 0; i < heights.Length; i++)
                vertices[i].Position.Y = heights[i];
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
