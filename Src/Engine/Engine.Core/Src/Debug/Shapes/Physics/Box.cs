using System.Numerics;
using Engine.Graphics.Vertices;
using Engine.Physics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Debug.Shapes.Physics
{
    public class DebugPhysicsBox : IDebugShape
    {
        private Box shape;
        private Matrix4x4 worldMatrix;
        private VertexPositionColor[] vertices;
        private short[] indices;

        private float lastWidth = float.MinValue;
        private float lastHeight = float.MinValue;
        private float lastLength = float.MinValue;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugPhysicsBox(Box shape)
        {
            this.shape = shape;
            Update();
        }

        public void Update()
        {
            worldMatrix = shape.SimulationTransform.Matrix;

            if (lastWidth != shape.Width || lastHeight != shape.Height || lastLength != shape.Length)
            {
                (vertices, indices) = ShapeGenerator.Box(shape.Width, shape.Height, shape.Length, shape.IsDynamic ? Color.Red : Color.Blue);
                lastWidth = shape.Width;
                lastHeight = shape.Height;
                lastLength = shape.Length;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 0,
            vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}