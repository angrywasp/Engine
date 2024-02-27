using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugLightCone : IDebugShape
    {
        protected VertexPositionColor[] vertices;
        protected short[] indices;

        public Matrix4x4 WorldMatrix {get; set; }

        public DebugLightCone(float radius, float length, Color color)
        {
            (vertices, indices) = ShapeGenerator.LightCone(radius, length, 32, color);
        }

        public void Update() { }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 
                0, vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}