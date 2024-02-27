using System.Numerics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Debug.Shapes.Physics
{
    public class DebugPhysicsMesh : IDebugShape
    {
        private Engine.Physics.Collidables.Mesh shape;
        private Matrix4x4 worldMatrix;
        private VertexPositionColor[] vertices;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugPhysicsMesh(Engine.Physics.Collidables.Mesh shape, Buffer<Triangle> buffer)
        {
            this.shape = shape;
            (vertices, _) = ShapeGenerator.TriangleBuffer(buffer, shape.IsDynamic ? Color.Red : Color.Blue);
            worldMatrix = shape.SimulationTransform.Matrix;
        }

        public void Update()
        {
            worldMatrix = shape.SimulationTransform.Matrix;
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserPrimitives(GLPrimitiveType.Triangles, vertices, 0, vertices.Length, VertexPositionColor.VertexDeclaration);
            //graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 0,
            //vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}