using System.Numerics;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine
{
    public class QuadRenderer
    {
        private VertexPositionTexture[] vbQuadHalfPixelOffset = null;
        GraphicsDevice graphicsDevice;
        private VertexBuffer vBuffer;
        private VertexBuffer vBufferHp;
        private IndexBuffer iBuffer;

        public QuadRenderer(GraphicsDevice graphicsDevice, Vector2i size)
        {
            this.graphicsDevice = graphicsDevice;

            vBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.None);
            vBuffer.SetData<VertexPositionTexture>(new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 0)), //top left
                new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 0)), //top right
                new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 1)), //bottom left
                new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 1)) //bottom right
            });

            vbQuadHalfPixelOffset = new VertexPositionTexture[]
            { 
                new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 0)), //top left
                new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 0)), //top right
                new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 1)), //bottom left
                new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 1)) //top right
            };

            vBufferHp = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.None);
            vBufferHp.SetData<VertexPositionTexture>(vbQuadHalfPixelOffset);

            //Draw bottom left triangle follwed by top right
            iBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            iBuffer.SetData<short>(new short[] { 0, 3, 2, 0, 1, 3 });
            UpdateHalfPixelOffset(size.ToVector2());
        }

        public void RenderQuad()
        {
            graphicsDevice.SetVertexBuffer(vBuffer);
            graphicsDevice.SetIndexBuffer(iBuffer);
            graphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 6);
        }

        public void UpdateHalfPixelOffset(Vector2 sz)
        {
            float px = 1.0f / sz.X;
            float py = 1.0f / sz.Y;

            vbQuadHalfPixelOffset[0].Position.X = -1 + px;
            vbQuadHalfPixelOffset[0].Position.Y = -1 - py;

            vbQuadHalfPixelOffset[1].Position.X = 1 + px;
            vbQuadHalfPixelOffset[1].Position.Y = -1 - py;

            vbQuadHalfPixelOffset[2].Position.X = -1 + px;
            vbQuadHalfPixelOffset[2].Position.Y = 1 - py;

            vbQuadHalfPixelOffset[3].Position.X = 1 + px;
            vbQuadHalfPixelOffset[3].Position.Y = 1 - py;

            vBufferHp.SetData<VertexPositionTexture>(vbQuadHalfPixelOffset);
        }

        public void RenderQuadHalfPixelOffset()
        {
            //RenderQuad();
            graphicsDevice.SetVertexBuffer(vBufferHp);
            graphicsDevice.SetIndexBuffer(iBuffer);
            graphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 6);
        }
    }
}
