using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugBoundingSphere : IDebugShape
    {
        private BoundingSphere sphere;
        private VertexPositionColor[] vertices;
        private short[] indices;
        private Matrix4x4 worldMatrix;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugBoundingSphere(BoundingSphere boundingSphere, Color color)
        {
            this.sphere = boundingSphere;

            worldMatrix = Matrix4x4.CreateTranslation(boundingSphere.Center);
            (vertices, indices) = ShapeGenerator.BoundingSphere(boundingSphere, color);
        }

        public void Update(BoundingSphere sphere)
        {
            if (sphere.Radius != this.sphere.Radius)
                ShapeGenerator.UpdateSphere(sphere.Radius, 32, ref vertices);

            worldMatrix = Matrix4x4.CreateTranslation(sphere.Center);

            this.sphere = sphere;
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 
                0, vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}
