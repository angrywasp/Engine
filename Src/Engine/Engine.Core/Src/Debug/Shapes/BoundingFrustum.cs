using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugBoundingFrustum : IDebugShape
    {
        private BoundingFrustum frustum;
        protected VertexPositionColor[] vertices;
        protected short[] indices;

        public Matrix4x4 WorldMatrix => Matrix4x4.Identity;

        public DebugBoundingFrustum(BoundingFrustum boundingFrustum, Color color)
        {
            this.frustum = boundingFrustum;
            (vertices, indices) = ShapeGenerator.BoundingFrustum(boundingFrustum, color);
        }

        public void Update(BoundingFrustum frustum)
        {
            if (frustum == this.frustum)
                return;

            var corners = frustum.GetCorners();
            for (int i = 0; i < corners.Length; i++)
                vertices[i].Position = corners[i];

            this.frustum = frustum;
        }

        public void Draw(GraphicsDevice graphicsDevice) =>
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Lines, vertices, 
                0, vertices.Length, indices, 0, indices.Length, VertexPositionColor.VertexDeclaration);
    }
}
