using System.Numerics;
using Engine.Graphics.Vertices;
using Engine.Physics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Debug.Shapes.Physics
{
    public class DebugPhysicsSphere : IDebugShape
    {
        private Sphere shape;
        private Matrix4x4 worldMatrix;
        private VertexPositionColor[] vertices;
        private short[] indices;

        private float lastRadius = float.MinValue;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugPhysicsSphere(Sphere shape)
        {
            this.shape = shape;
            Update();
        }

        public void Update()
        {
            worldMatrix = shape.SimulationTransform.Matrix;

            if (lastRadius != shape.Radius)
            {
                (vertices, indices) = ShapeGenerator.Sphere(shape.Radius, 32, shape.IsDynamic ? Color.Red : Color.Blue);
                lastRadius = shape.Radius;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 0,
            vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}