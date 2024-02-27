using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Cameras;
using Engine.Content.Model;
using Engine.Content.Model.Instance;
using Engine.Content.Model.Template;
using Engine.Graphics.Materials;
using Engine.Interfaces;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.World.Objects
{
    public class DynamicInstancingGroup
    {
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Position, 4)
        );

        private EngineCore engine;
        private SharedMeshData meshData;
        private List<(MeshMaterial Material, SubMeshTemplate Mesh)> subMeshes;
        private List<IDrawableObjectInstanced> allMeshes = new List<IDrawableObjectInstanced>();
        private DynamicVertexBuffer instanceVertexBuffer;
        private Matrix4x4[] instanceTransforms;

        public int Count => allMeshes.Count;

        public static DynamicInstancingGroup Create(EngineCore engine, MeshInstance mesh)
        {
            Threading.EnsureUIThread();

            if (mesh.IsSkinned)
                throw new Exception("Skinned meshes are not supported for instancing");

            return new DynamicInstancingGroup {
                engine = engine,
                meshData = mesh.SharedData,
                subMeshes = mesh.SubMeshes.Select(x => (x.Material, x.Template)).ToList()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IDrawableObjectInstanced mesh)
        {
            allMeshes.Add(mesh);
            instanceVertexBuffer = new DynamicVertexBuffer(engine.GraphicsDevice, instanceVertexDeclaration, allMeshes.Count, BufferUsage.WriteOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IDrawableObjectInstanced mesh)
        {
            allMeshes.Remove(mesh);
            instanceVertexBuffer = new DynamicVertexBuffer(engine.GraphicsDevice, instanceVertexDeclaration, allMeshes.Count, BufferUsage.WriteOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBuffers(GBuffer gBuffer, LBuffer lBuffer)
        {
            foreach (var m in subMeshes)
                m.Material.SetBuffers(gBuffer, lBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Camera camera)
        {
            //todo: shouldn't update if camera hasn't moved
            instanceTransforms = allMeshes.Where(x => x.ShouldDraw(camera)).Select(x => x.GlobalTransform.Matrix).ToArray();
            if (instanceTransforms.Length > 0)
                instanceVertexBuffer.SetData(instanceTransforms, 0, instanceTransforms.Length, SetDataOptions.None);
            
            foreach (var m in subMeshes)
                m.Material.UpdateTransform(Matrix4x4.Identity, camera.View, camera.Projection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4[] GetShadowCasters(BoundingFrustum frustum) => allMeshes.Where(x => x.Mesh.Intersects(frustum)).Select(x => x.GlobalTransform.Matrix).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4[] GetShadowCasters(BoundingSphere sphere) => allMeshes.Where(x => x.Mesh.Intersects(sphere)).Select(x => x.GlobalTransform.Matrix).ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderToGBuffer()
        {
            engine.GraphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(meshData.PositionBuffer),
                new VertexBufferBinding(meshData.NormalBuffer),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
            );

            engine.GraphicsDevice.SetIndexBuffer(meshData.IndexBuffer);
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (var subMesh in subMeshes)
            {
                subMesh.Material.Apply("RenderToGBufferInstanced");
                engine.GraphicsDevice.DrawInstancedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount, 0, instanceVertexBuffer.VertexCount);
            }

            /*foreach (var subMesh in subMeshes)
            {
                foreach (var i in instanceTransforms)
                {
                    subMesh.Material.Effect.Parameters["World"].SetValue(i);
                    subMesh.Material.Apply("RenderToGBuffer");
                    engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount);
                }
            }*/
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reconstruct()
        {
            engine.GraphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(meshData.PositionBuffer),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
            );

            engine.GraphicsDevice.SetIndexBuffer(meshData.IndexBuffer);
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (var subMesh in subMeshes)
            {
                subMesh.Material.Apply("ReconstructInstanced");
                engine.GraphicsDevice.DrawInstancedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount, 0, instanceVertexBuffer.VertexCount);
            }

            /*foreach (var subMesh in subMeshes)
            {
                foreach (var i in instanceTransforms)
                {
                    subMesh.Material.Effect.Parameters["World"].SetValue(i);
                    subMesh.Material.Apply("Reconstruct");
                    engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount);
                }
            }*/
        }

        public void ShadowMap(Matrix4x4[] shadowCasters, Matrix4x4 lightView, Matrix4x4 lightProjection)
        {
            if (shadowCasters.Length == 0)
                return;

            instanceVertexBuffer.SetData(shadowCasters, 0, shadowCasters.Length, SetDataOptions.None);

            engine.GraphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(meshData.PositionBuffer),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
            );

            engine.GraphicsDevice.SetIndexBuffer(meshData.IndexBuffer);
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            foreach (var subMesh in subMeshes)
            {
                subMesh.Material.Effect.Parameters["View"].SetValue(lightView);
                subMesh.Material.Effect.Parameters["Projection"].SetValue(lightProjection);

                subMesh.Material.Apply("ShadowMapInstanced");
                engine.GraphicsDevice.DrawInstancedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount, 0, instanceVertexBuffer.VertexCount);
            }

            /*foreach (var subMesh in subMeshes)
            {
                subMesh.Material.Effect.Parameters["View"].SetValue(lightView);
                subMesh.Material.Effect.Parameters["Projection"].SetValue(lightProjection);

                foreach (var i in shadowCasters)
                {
                    subMesh.Material.Effect.Parameters["World"].SetValue(i);
                    subMesh.Material.Apply("ShadowMap");
                    engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, subMesh.Mesh.BaseVertex, subMesh.Mesh.StartIndex, subMesh.Mesh.IndexCount);
                }
            }*/
        }
    }
}