using Microsoft.Xna.Framework;
using System.Numerics;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using BepuPhysics.Collidables;
using BepuPhysics;

namespace Engine.Debug.Shapes.Physics
{
    public class DebugPhysicsCharacterCapsule : IDebugShape
    {
        private Capsule shape;
        private BodyReference bodyReference;
        private Matrix4x4 worldMatrix;
        private VertexPositionColor[] vertices;
        private short[] indices;

        private float lastRadius = float.MinValue;
        private float lastLength = float.MinValue;

        public Matrix4x4 WorldMatrix => worldMatrix;

        public DebugPhysicsCharacterCapsule(ref Capsule shape, ref BodyReference bodyReference)
        {
            this.shape = shape;
            this.bodyReference = bodyReference;
            Update();
        }

        public void Update()
        {
            bodyReference.GetDescription(out BodyDescription description);
            worldMatrix = Matrix4x4.CreateTranslation(description.Pose.Position);

            if (lastRadius != shape.Radius || lastLength != shape.Length)
            {
                (vertices, indices) = ShapeGenerator.Capsule(shape.Radius, shape.Length, 32, Color.Purple);
                lastRadius = shape.Radius;
                lastLength = shape.Length;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 0,
            vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}