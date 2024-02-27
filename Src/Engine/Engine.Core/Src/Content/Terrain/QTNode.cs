using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;
using Engine.Graphics.Vertices;

namespace Engine.Content.Terrain
{
    public class QTNode
    {
        private int width;
        private int height;
        private bool isEndNode;
        private BoundingBox nodeBoundingBox;
        private GraphicsDevice device;

        private QTNode nodeUL;
        private QTNode nodeUR;
        private QTNode nodeLL;
        private QTNode nodeLR;

        private bool visible;

        private VertexBuffer positionVertexBuffer;
        private VertexBuffer normalVertexBuffer;
        private IndexBuffer indexBuffer;
        private EngineCore engine;

        public static int NodesVisible;

        public QTNode(EngineCore engine, VertexPositionTexture[,] positions, VertexNormalTangentBinormal[,] normals, GraphicsDevice device, int maxSize)
        {
            this.engine = engine;
            this.device = device;

            width = positions.GetLength(0);
            height = positions.GetLength(1);
            nodeBoundingBox = CreateBoundingBox(positions);

            isEndNode = width <= maxSize;
            isEndNode &= height <= maxSize;

            if (isEndNode)
            {
                var vertexBuffers = Reshape2Dto1D(positions, normals);
                CreateBuffers(vertexBuffers.Positions, vertexBuffers.Normals, CreateTerrainIndices(width, height),
                    out positionVertexBuffer, out normalVertexBuffer, out indexBuffer, device);
            }
            else
                CreateChildNodes(positions, normals, maxSize);
        }

        private int[] CreateTerrainIndices(int width, int height)
        {
            int[] i = new int[(width - 1) * (height - 1) * 6];
            int counter = 0;
            for (int y = 0; y < height - 1; y++)
                for (int x = 0; x < width - 1; x++)
                {
                    int lowerLeft = x + y * width;
                    int lowerRight = (x + 1) + y * width;
                    int topLeft = x + (y + 1) * width;
                    int topRight = (x + 1) + (y + 1) * width;

                    i[counter++] = lowerLeft;
                    i[counter++] = topLeft;
                    i[counter++] = lowerRight;

                    i[counter++] = lowerRight;
                    i[counter++] = topLeft;
                    i[counter++] = topRight;
                }

            return i;
        }

        public static void CreateBuffers(VertexPositionTexture[] positions, VertexNormalTangentBinormal[] normals, int[] indices, out VertexBuffer positionVertexBuffer, out VertexBuffer normalVertexBuffer, out IndexBuffer indexBuffer, GraphicsDevice device)
        {
            positionVertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, positions.Length, BufferUsage.WriteOnly);
            positionVertexBuffer.SetData(positions);

            normalVertexBuffer = new VertexBuffer(device, VertexNormalTangentBinormal.VertexDeclaration, normals.Length, BufferUsage.WriteOnly);
            normalVertexBuffer.SetData(normals);

            indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        private BoundingBox CreateBoundingBox(VertexPositionTexture[,] vertexArray)
        {
            List<Vector3> pointList = new List<Vector3>();
            foreach (var vertex in vertexArray)
                pointList.Add(vertex.Position);

            BoundingBox nodeBoundingBox = BoundingBox.CreateFromPoints(pointList);
            return nodeBoundingBox;
        }

        private void CreateChildNodes(VertexPositionTexture[,] positions, VertexNormalTangentBinormal[,] normals, int maxSize)
        {
            {
                VertexPositionTexture[,] pa = new VertexPositionTexture[width / 2 + 1, height / 2 + 1];
                VertexNormalTangentBinormal[,] na = new VertexNormalTangentBinormal[width / 2 + 1, height / 2 + 1];
                for (int w = 0; w < width / 2 + 1; w++)
                    for (int h = 0; h < height / 2 + 1; h++)
                    {
                        pa[w, h] = positions[w, h];
                        na[w, h] = normals[w, h];
                    }

                nodeUL = new QTNode(engine, pa, na, device, maxSize);
            }

            {
                VertexPositionTexture[,] pa = new VertexPositionTexture[width - (width / 2), height / 2 + 1];
                VertexNormalTangentBinormal[,] na = new VertexNormalTangentBinormal[width - (width / 2), height / 2 + 1];
                for (int w = 0; w < width - (width / 2); w++)
                    for (int h = 0; h < height / 2 + 1; h++)
                    {
                        pa[w, h] = positions[width / 2 + w, h];
                        na[w, h] = normals[width / 2 + w, h];
                    }

                nodeUR = new QTNode(engine, pa, na, device, maxSize);
            }

            {
                VertexPositionTexture[,] pa = new VertexPositionTexture[width / 2 + 1, height - (height / 2)];
                VertexNormalTangentBinormal[,] na = new VertexNormalTangentBinormal[width / 2 + 1, height - (height / 2)];
                for (int w = 0; w < width / 2 + 1; w++)
                    for (int h = 0; h < height - (height / 2); h++)
                    {
                        pa[w, h] = positions[w, height / 2 + h];
                        na[w, h] = normals[w, height / 2 + h];
                    }

                nodeLL = new QTNode(engine, pa, na, device, maxSize);
            }

            {

                VertexPositionTexture[,] pa = new VertexPositionTexture[width - (width / 2), height - (height / 2)];
                VertexNormalTangentBinormal[,] na = new VertexNormalTangentBinormal[width - (width / 2), height - (height / 2)];
                for (int w = 0; w < width - (width / 2); w++)
                    for (int h = 0; h < height - (height / 2); h++)
                    {
                        pa[w, h] = positions[width / 2 + w, height / 2 + h];
                        na[w, h] = normals[width / 2 + w, height / 2 + h];
                    }

                nodeLR = new QTNode(engine, pa, na, device, maxSize);
            }
        }

        private (VertexPositionTexture[] Positions, VertexNormalTangentBinormal[] Normals) Reshape2Dto1D(VertexPositionTexture[,] p2D, VertexNormalTangentBinormal[,] n2D)
        {
            int width = p2D.GetLength(0);
            int height = p2D.GetLength(1);

            VertexPositionTexture[] p1D = new VertexPositionTexture[width * height];
            VertexNormalTangentBinormal[] n1D = new VertexNormalTangentBinormal[width * height];

            int i = 0;
            for (int z = 0; z < height; z++)
                for (int x = 0; x < width; x++)
                {
                    p1D[i] = p2D[x, z];
                    n1D[i] = n2D[x, z];
                    i++;
                }

            return (p1D, n1D);
        }

        private void DrawCurrentNode(bool programNeedsNormalBuffer)
        {
            if (programNeedsNormalBuffer)
                device.SetVertexBuffers(new VertexBufferBinding(positionVertexBuffer), new VertexBufferBinding(normalVertexBuffer));
            else
                device.SetVertexBuffer(positionVertexBuffer);

            device.SetIndexBuffer(indexBuffer);
            //device.RasterizerState = material.Rs;
            device.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, indexBuffer.IndexCount);
        }

        public void Update(Matrix4x4 worldMatrix, BoundingFrustum cameraFrustum)
        {
            ContainmentType cameraNodeContainment = cameraFrustum.Contains(transformedBBox);
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                visible = true;
                if (isEndNode)
                {
                    NodesVisible++;
                    return;
                }
                else
                {
                    nodeUL.Update(worldMatrix, cameraFrustum);
                    nodeUR.Update(worldMatrix, cameraFrustum);
                    nodeLL.Update(worldMatrix, cameraFrustum);
                    nodeLR.Update(worldMatrix, cameraFrustum);
                }
            }
        }

        public void Draw(bool programNeedsNormalBuffer, bool setVisibleFalse, Matrix4x4 worldMatrix, BoundingFrustum cameraFrustum)
        {
            if (visible)
            {
                visible = !setVisibleFalse;
                if (isEndNode)
                    DrawCurrentNode(programNeedsNormalBuffer);
                else
                {
                    nodeUL.Draw(programNeedsNormalBuffer, setVisibleFalse, worldMatrix, cameraFrustum);
                    nodeUR.Draw(programNeedsNormalBuffer, setVisibleFalse, worldMatrix, cameraFrustum);
                    nodeLL.Draw(programNeedsNormalBuffer, setVisibleFalse, worldMatrix, cameraFrustum);
                    nodeLR.Draw(programNeedsNormalBuffer, setVisibleFalse, worldMatrix, cameraFrustum);
                }
            }
        }

        BoundingBox transformedBBox;
        public void UpdateBoundingBox(Matrix4x4 transform)
        {
            transformedBBox = TransformBoundingBox(nodeBoundingBox, transform);

            if (isEndNode)
                return;
            else
            {
                nodeUL.UpdateBoundingBox(transform);
                nodeUR.UpdateBoundingBox(transform);
                nodeLL.UpdateBoundingBox(transform);
                nodeLR.UpdateBoundingBox(transform);
            }
        }

        private BoundingBox TransformBoundingBox(BoundingBox origBox, Matrix4x4 matrix)
        {
            Vector3 origCorner1 = origBox.Min;
            Vector3 origCorner2 = origBox.Max;

            Vector3 transCorner1 = Vector3.Transform(origCorner1, matrix);
            Vector3 transCorner2 = Vector3.Transform(origCorner2, matrix);

            return new BoundingBox(transCorner1, transCorner2);
        }
    }
}
