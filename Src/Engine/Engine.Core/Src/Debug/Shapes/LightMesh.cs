using Engine.Content.Model.Instance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Engine.Debug.Shapes
{
    public class DebugMesh : IDebugShape
    {
        protected MeshInstance lightMesh;
        protected int subMeshIndex;

        public Matrix4x4 WorldMatrix { get; set;} = Matrix4x4.Identity;

        public DebugMesh(MeshInstance lightMesh, int subMeshIndex, Color color)
        {
            this.lightMesh = lightMesh;
            this.subMeshIndex = subMeshIndex;
        }

        public void Update() { }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetVertexBuffer(lightMesh.SharedData.PositionBuffer);
            graphicsDevice.SetIndexBuffer(lightMesh.SharedData.IndexBuffer);
            var s = lightMesh.SubMeshes[subMeshIndex];
            graphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, s.BaseVertex, s.StartIndex, s.IndexCount);
        }
            //graphicsDevice.DrawUserPrimitives<VertexPositionColor>(GLPrimitiveType.Triangles, this.vertices, 0, this.vertices.Length, MeshVertex.VertexDeclaration);
    }
}