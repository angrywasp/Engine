using System.Collections.Generic;
using System.Numerics;
using Engine.Cameras;
using Engine.Content.Model.Instance;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using Newtonsoft.Json;

namespace Engine.World.Objects
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstancingGroup
    {
        private EngineCore engine;
        private DynamicVertexBuffer instanceVertexBuffer;
        private List<InstancingGroupItem> items = new List<InstancingGroupItem>();

        [JsonProperty] public string MeshPath { get; set; }

        [JsonProperty] public List<Matrix4x4> InstanceTransforms { get; set; } = new List<Matrix4x4>(64);

        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Position, 4)
        );

        public void Load(EngineCore engine, MeshInstance mesh)//, string meshPath)
        {
            this.engine = engine;

            foreach (var subMesh in mesh.SubMeshes)
                items.Add(InstancingGroupItem.Create(engine, mesh, subMesh));

            UpdateVertexBuffers();
        }

        public void AddMesh(Matrix4x4 transform)
        {
            InstanceTransforms.Add(transform);
            UpdateVertexBuffers();
        }

        public void UpdateVertexBuffers()
        {
            if (InstanceTransforms.Count == 0)
                return;

            Threading.BlockOnUIThread(() => {
                instanceVertexBuffer = new DynamicVertexBuffer(engine.GraphicsDevice, instanceVertexDeclaration, InstanceTransforms.Count, BufferUsage.WriteOnly);
                instanceVertexBuffer.SetData<Matrix4x4>(InstanceTransforms.ToArray(), 0, InstanceTransforms.Count, SetDataOptions.Discard);
            });
        }

        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer)
        {
            foreach (var i in items)
                i.Material.SetBuffers(gBuffer, lBuffer);
        }

        public void DrawCommon(bool programNeedsNormalBuffer, string program, Camera camera)
        {
            foreach (var i in items)
            {
                if (instanceVertexBuffer == null)
                    continue;

                if (instanceVertexBuffer.VertexCount == 0)
                    continue;

                if (programNeedsNormalBuffer)
                {
                    engine.GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(i.PositionVertexBuffer),
                        new VertexBufferBinding(i.NormalVertexBuffer),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );
                }
                else
                {
                    engine.GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(i.PositionVertexBuffer),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );
                }

                engine.GraphicsDevice.SetIndexBuffer(i.IndexBuffer);

                i.Material.UpdateTransform(Matrix4x4.Identity, camera.View, camera.Projection);
                engine.GraphicsDevice.RasterizerState = i.Material.RasterizerState;
                i.Material.Apply(program);
                engine.GraphicsDevice.DrawInstancedPrimitives(GLPrimitiveType.Triangles, 0, 0, i.IndexCount, 0, instanceVertexBuffer.VertexCount);
            }
        }

        public void DrawShadow(Matrix4x4 lightView, Matrix4x4 lightProjection)
        {
            foreach (var i in items)
            {
                if (instanceVertexBuffer == null)
                    continue;

                if (instanceVertexBuffer.VertexCount == 0)
                    continue;
                    
                engine.GraphicsDevice.SetVertexBuffers(
                    new VertexBufferBinding(i.PositionVertexBuffer),
                    new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                );

                engine.GraphicsDevice.SetIndexBuffer(i.IndexBuffer);

                if (i.Material.DoubleSided)
                    engine.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                else
                    engine.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

                i.Material.UpdateTransform(Matrix4x4.Identity, lightView, lightProjection);
                i.Material.Apply("ShadowMapInstanced");

                engine.GraphicsDevice.DrawInstancedPrimitives(GLPrimitiveType.Triangles, 0, 0, i.IndexCount, 0, instanceVertexBuffer.VertexCount);
            }
        }

        public void RenderToGBuffer(Camera camera) => DrawCommon(true, "RenderToGBufferInstanced", camera);

        public void ReconstructShading(Camera camera) => DrawCommon(false, "ReconstructInstanced", camera);

        public void ReconstructShadingDiffuseLight(Camera camera) => DrawCommon(false, "ReconstructNoLightingInstanced", camera);
    }
}