using System.Numerics;
using Engine.Graphics.Vertices;
using Engine.Physics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Debug.Shapes.Physics
{
    public class DebugPhysicsCylinder : IDebugShape
    {
        private Cylinder shape;
        private Matrix4x4 worldMatrix;
        private VertexPositionColor[] vertices;
        private short[] indices;

        private float lastRadius = float.MinValue;
        private float lastLength = float.MinValue;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugPhysicsCylinder(Cylinder shape)
        {
            this.shape = shape;
            Update();
        }

        public void Update()
        {
            worldMatrix = this.shape.SimulationTransform.Matrix;

            if (lastRadius != shape.Radius || lastLength != shape.Length)
            {
                (vertices, indices) = ShapeGenerator.Cylinder(shape.Radius, shape.Length, 32, shape.IsDynamic ? Color.Red : Color.Blue);
                lastRadius = shape.Radius;
                lastLength = shape.Length;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 0,
            vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}