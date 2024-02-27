using System.Diagnostics;
using Engine.Content.Model.Instance;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.World.Objects
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstancingGroupItem
    {
        public MeshMaterial Material;

        public VertexBuffer PositionVertexBuffer;
        public VertexBuffer NormalVertexBuffer;

        public IndexBuffer IndexBuffer;

        public int IndexCount;

        public static InstancingGroupItem Create(EngineCore engine, MeshInstance m, SubMeshInstance s)
        {
            Debugger.Break();
            /*InstancingGroupItem item = new InstancingGroupItem();

            item.Material = ContentLoader.LoadMaterial(s.MaterialPath, m.Type == Mesh_Type.Skinned);
            await item.Material.LoadAsync(engine);

            item.IndexCount = s.IndexCount;

            var positions = m.Positions.Skip(s.BaseVertex).Take(s.NumVertices).ToArray();
            var normals = m.Normals.Skip(s.BaseVertex).Take(s.NumVertices).ToArray();
            var indices = m.Indices.Skip(s.StartIndex).Take(s.IndexCount).ToArray();

            item.PositionVertexBuffer = new VertexBuffer(engine.GraphicsDevice, VertexPositionTexture.VertexDeclaration, positions.Length, BufferUsage.None);
            item.NormalVertexBuffer = new VertexBuffer(engine.GraphicsDevice, VertexNormalTangentBinormal.VertexDeclaration, normals.Length, BufferUsage.None);

            item.PositionVertexBuffer.SetData(positions.ToArray());
            item.NormalVertexBuffer.SetData(normals.ToArray());

            item.IndexBuffer = new IndexBuffer(engine.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
            item.IndexBuffer.SetData(indices.ToArray());*/

            return null;
        }
    }
}