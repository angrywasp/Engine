using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Content.Model
{
    public class SharedMeshData
    {
        public VertexBuffer PositionBuffer;
        public VertexBuffer NormalBuffer;
        public VertexBuffer SkinBuffer;

        public VertexBufferBinding[] RenderToGBufferBinding;
        public VertexBufferBinding[] ReconstructBinding;
        public VertexBufferBinding[] ShadowBinding;
        public IndexBuffer IndexBuffer;

        public SharedMeshData(GraphicsDevice graphicsDevice, VertexPositionTexture[] positionVertices, int[] indices)
        {
            PositionBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, positionVertices.Length, BufferUsage.None);
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            RenderToGBufferBinding = ShadowBinding = ReconstructBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(PositionBuffer)
            };

            Threading.BlockOnUIThread(() =>
            {
                PositionBuffer.SetData<VertexPositionTexture>(positionVertices);
                IndexBuffer.SetData<int>(indices);
            });
        }

        public SharedMeshData(GraphicsDevice graphicsDevice, VertexPositionTexture[] positionVertices, VertexNormalTangentBinormal[] normalVertices, int[] indices)
        {
            PositionBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, positionVertices.Length, BufferUsage.None);
            NormalBuffer = new VertexBuffer(graphicsDevice, typeof(VertexNormalTangentBinormal), normalVertices.Length, BufferUsage.None);
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            RenderToGBufferBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(PositionBuffer),
                new VertexBufferBinding(NormalBuffer)
            };

            ShadowBinding = ReconstructBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(PositionBuffer)
            };

            Threading.BlockOnUIThread(() =>
            {
                PositionBuffer.SetData<VertexPositionTexture>(positionVertices);
                NormalBuffer.SetData<VertexNormalTangentBinormal>(normalVertices);
                IndexBuffer.SetData<int>(indices);
            });
        }

        public SharedMeshData(GraphicsDevice graphicsDevice, VertexPositionTexture[] positionVertices, VertexNormalTangentBinormal[] normalVertices, VertexSkinWeight[] skinVertices, int[] indices)
        {
            PositionBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, positionVertices.Length, BufferUsage.None);
            NormalBuffer = new VertexBuffer(graphicsDevice, typeof(VertexNormalTangentBinormal), normalVertices.Length, BufferUsage.None);
            SkinBuffer = new VertexBuffer(graphicsDevice, typeof(VertexSkinWeight), skinVertices.Length, BufferUsage.None);
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            RenderToGBufferBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(PositionBuffer),
                new VertexBufferBinding(NormalBuffer),
                new VertexBufferBinding(SkinBuffer)
            };

            ShadowBinding = ReconstructBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(PositionBuffer),
                new VertexBufferBinding(SkinBuffer)
            };

            Threading.BlockOnUIThread(() =>
            {
                PositionBuffer.SetData<VertexPositionTexture>(positionVertices);
                NormalBuffer.SetData<VertexNormalTangentBinormal>(normalVertices);
                SkinBuffer.SetData<VertexSkinWeight>(skinVertices);
                IndexBuffer.SetData<int>(indices);
            });
        }
    }
}