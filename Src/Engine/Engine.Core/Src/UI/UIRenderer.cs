using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MonoGame.OpenGL;
using System.Numerics;
using Engine.Graphics.Vertices;

namespace Engine.UI
{
    public class UIRenderer
    {
        private GraphicsDevice graphicsDevice;
        private UiEffect effect;

        private readonly RasterizerState DefaultRasterizerState = new RasterizerState()
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            ScissorTestEnable = true
        };
        private static readonly BlendState DefaultBlendState = BlendState.AlphaBlend;

        public UIRenderer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            effect = new UiEffect(graphicsDevice);
            effect.Load();
        }

#region DeferRectangle overloads

        public void DeferRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size, ref List<VertexPositionColorTexture> vertices, ref List<int> indices)
        {
            int baseIndex = vertices.Count;
            indices.AddRange(new int[]
            {
                baseIndex, (baseIndex + 1), (baseIndex + 2), (baseIndex + 1), (baseIndex + 3), (baseIndex + 2)
            });

            vertices.AddRange(new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, Vector2.Zero), //top left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y, 0), color, Vector2.UnitX), //top right
                new VertexPositionColorTexture(new Vector3(position.X, position.Y + size.Y, 0), color, Vector2.UnitY), //bottom left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, 0), color, Vector2.One) //bottom right
            });
        }

        public void DeferRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size, Rectangle region, ref List<VertexPositionColorTexture> vertices, ref List<int> indices)
        {
            int baseIndex = vertices.Count;
            indices.AddRange(new int[]
            {
                baseIndex, (baseIndex + 1), (baseIndex + 2), (baseIndex + 1), (baseIndex + 3), (baseIndex + 2)
            });

            float px = 1.0f / tex.Width;
            float py = 1.0f / tex.Height;

            float tcX = region.X * px;
            float tcY = region.Y * py;
            float tcW = tcX + (region.Width * px);
            float tcH = tcY + (region.Height * py);

            vertices.AddRange(new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, new Vector2(tcX, tcY)), //top left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y, 0), color, new Vector2(tcW, tcY)), //top right
                new VertexPositionColorTexture(new Vector3(position.X, position.Y + size.Y, 0), color, new Vector2(tcX, tcH)), //bottom left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, 0), color, new Vector2(tcW, tcH)) //bottom right
            });
        }

        public void DeferRectangle(Color color, Vector2i position, Vector2i size, ref List<VertexPositionColorTexture> vertices, ref List<int> indices)
        {
            int baseIndex = vertices.Count;
            indices.AddRange(new int[]
            {
                baseIndex, (baseIndex + 1), (baseIndex + 2), (baseIndex + 1), (baseIndex + 3), (baseIndex + 2)
            });

            vertices.AddRange(new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, Vector2.Zero), //top left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y, 0), color, Vector2.UnitX), //top right
                new VertexPositionColorTexture(new Vector3(position.X, position.Y + size.Y, 0), color, Vector2.UnitY), //bottom left
                new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, 0), color, Vector2.One) //bottom right
            });
        }

        public void DeferRectangle(Color color, Vector2i[] points, ref List<VertexPositionColorTexture> vertices, ref List<int> indices)
        {
            int baseIndex = vertices.Count;
            indices.AddRange(new int[]
            {
                baseIndex, (baseIndex + 1), (baseIndex + 2), (baseIndex + 1), (baseIndex + 3), (baseIndex + 2)
            });

            //todo: do something about the conversions ToVector2
            vertices.AddRange(new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(points[0].ToVector2(), 0), color, Vector2.Zero), //top left
                new VertexPositionColorTexture(new Vector3(points[1].ToVector2(), 0), color, Vector2.UnitX), //top right
                new VertexPositionColorTexture(new Vector3(points[2].ToVector2(), 0), color, Vector2.UnitY), //bottom left
                new VertexPositionColorTexture(new Vector3(points[3].ToVector2(), 0), color, Vector2.One) //bottom right
            });
        }

#endregion

#region DrawRectangle overloads

        public void DrawRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size)
        {
            CreateRectangle(position, color, size, out VertexPositionColorTexture[] vertices, out short[] indices);

            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

            effect.Texture = tex;

            effect.Apply("Texture");
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, vertices, 0, 4, indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
        }

        public void DrawRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size, BlendState bs)
        {
            short[] indices;
            VertexPositionColorTexture[] vertices;
            CreateRectangle(position, color, size, out vertices, out indices);

            graphicsDevice.BlendState = bs;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = DefaultRasterizerState;

            effect.Texture = tex;

            effect.Apply("Texture");
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, vertices, 0, 4, indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
        }

        public void DrawRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size, Rectangle region)
        {
            short[] indices;
            VertexPositionColorTexture[] vertices;
            CreateRectangle(tex, color, position, size, region, out vertices, out indices);

            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;

            effect.Texture = tex;

            effect.Apply("Texture");
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, vertices, 0, 4, indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
        }

        public void DrawRectangle(Color color, Vector2i position, Vector2i size)
        {
            short[] indices;
            VertexPositionColorTexture[] vertices;
            CreateRectangle(position, color, size, out vertices, out indices);

            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;

            effect.Apply("Color");
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, vertices, 0, 4, indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
        }

#endregion

#region DrawVertices overloads

        public void DrawVertices(VertexPositionColorTexture[] vertices, int[] indices, Texture2D texture)
        {
            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;

            effect.Texture = texture;

            effect.Apply("Texture");
            graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, vertices, 0, vertices.Length, indices, 0, indices.Length, VertexPositionColorTexture.VertexDeclaration);
        }

#endregion

        public void DrawString(string text, Vector2i position, Font font, Color color)
        {
            DrawString(text, position, font, color, out VertexPositionColorTexture[] vertices, out int[] indices);
            DeferText(vertices, indices, font.Texture);
        }

        public void DrawString(string text, Vector2i position, Font font, Color color, out VertexPositionColorTexture[] vertices, out int[] indices)
        {
            char[] chars = text.ToCharArray();
            vertices = new VertexPositionColorTexture[chars.Length * 4];
            indices = new int[chars.Length * 6];

            float px = 1.0f / font.Texture.Width;
            float py = 1.0f / font.Texture.Height;

            int x = 0, y = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                FontChar f = font[chars[i]];
                Vector2i s = f.Size;
                Vector2i p = f.Position;

                float tcX = p.X * px;
                float tcY = p.Y * py;
                float tcW = tcX + (s.X * px);
                float tcH = tcY + (s.Y * py);

                indices[y++] = x;
                indices[y++] = x + 1;
                indices[y++] = x + 2;
                indices[y++] = x + 1;
                indices[y++] = x + 3;
                indices[y++] = x + 2;

                vertices[x++] = new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, new Vector2(tcX, tcY)); //top left
                vertices[x++] = new VertexPositionColorTexture(new Vector3(position.X + s.X, position.Y, 0), color, new Vector2(tcW, tcY)); //top right
                vertices[x++] = new VertexPositionColorTexture(new Vector3(position.X, position.Y + s.Y, 0), color, new Vector2(tcX, tcH)); //bottom left
                vertices[x++] = new VertexPositionColorTexture(new Vector3(position.X + s.X, position.Y + s.Y, 0), color, new Vector2(tcW, tcH)); //bottom right

                position.X += f.Size.X + font.LetterSpacing;
            }
        }

        public class DeferredText
        {
            public Texture2D Texture;
            public Color Color;
            public List<VertexPositionColorTexture[]> Vertices = new List<VertexPositionColorTexture[]>();
            public List<int[]> Indices = new List<int[]>();
        }

        private Dictionary<int, DeferredText> deferredText = new Dictionary<int, DeferredText> ();

        public void DeferText(VertexPositionColorTexture[] vertices, int[] indices, Texture2D texture) => DeferText(deferredText, vertices, indices, texture);

        public void DeferText(Dictionary<int, DeferredText> dt, VertexPositionColorTexture[] vertices, int[] indices, Texture2D texture)
        {
            int hashCode = texture.GetHashCode();

            DeferredText d;

            if (dt.TryGetValue(hashCode, out d))
            {
                d.Vertices.Add(vertices);
                d.Indices.Add(indices);
            }
            else
            {
                d = new DeferredText
                {
                    Texture = texture,
                };
                d.Vertices.Add(vertices);
                d.Indices.Add(indices);
                dt.Add(hashCode, d);
            }
        }

        public void DrawDeferredText()
        {
            //todo: this can be optimized further by batching all the text into a single vertex/index buffer
            DrawDeferredText(deferredText);
        }

        public void DrawDeferredText(Dictionary<int, DeferredText> dt)
        {
            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;

            foreach (var i in dt.Values)
            {  
                effect.Texture = i.Texture;
                effect.Apply("Texture");

                for (int x = 0; x < i.Vertices.Count; x++)
                    graphicsDevice.DrawUserIndexedPrimitives(GLPrimitiveType.Triangles, i.Vertices[x], 0, 
                        i.Vertices[x].Length, i.Indices[x], 0, i.Indices[x].Length, VertexPositionColorTexture.VertexDeclaration);
            }
            
            dt.Clear();
        }

        public void DrawString(string text, Vector2i position, Font font, Color color, int caretCharPosition, out Vector2i caretDrawPosition)
        {
            graphicsDevice.BlendState = DefaultBlendState;
            graphicsDevice.RasterizerState = DefaultRasterizerState;
            caretDrawPosition = Vector2i.Zero;
            char[] chars = text.ToCharArray();
            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
            List<int> indices = new List<int>();
            
            for (int i = 0; i < chars.Length; i++)
            {
                FontChar f = font[chars[i]];
                Vector2i s = f.Size;
                Vector2i p = f.Position;

                DeferRectangle(font.Texture, color, position, s, new Rectangle(p.X, p.Y, s.X, s.Y), ref vertices, ref indices);
                
                if (caretCharPosition == i)
                    caretDrawPosition = new Vector2i(position.X, 0);
                position.X += f.Size.X + font.LetterSpacing;
            }

            if (caretCharPosition == chars.Length)
                caretDrawPosition = new Vector2i(position.X, 0);

            if (vertices.Count != 0)
                DrawVertices(vertices.ToArray(), indices.ToArray(), font.Texture);
        }
        
        public void SetBufferSize()
        {
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1);
            effect.Projection = projection;
        }

#region CreateRectangle overloads

        private void CreateRectangle(Vector2i position, Color color, Vector2i size, out VertexPositionColorTexture[] vertices, out short[] indices)
        {
            int x = position.X;
            int y = position.Y;

            indices = new short[] { 0, 1, 2, 1, 3, 2 };
            vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(x, y, 0), color, Vector2.Zero), //top left
                new VertexPositionColorTexture(new Vector3(x + size.X, y, 0), color, Vector2.UnitX), //top right
                new VertexPositionColorTexture(new Vector3(x, y + size.Y, 0), color, Vector2.UnitY), //bottom left
                new VertexPositionColorTexture(new Vector3(x + size.X, y + size.Y, 0), color, Vector2.One) //bottom right
            };
        }

        private void CreateRectangle(Texture2D tex, Color color, Vector2i position, Vector2i size, Rectangle region, out VertexPositionColorTexture[] vertices, out short[] indices)
        {
            float px = 1.0f / (float)tex.Width;
            float py = 1.0f / (float)tex.Height;

            float tcX = (float)region.X * px;
            float tcY = (float)region.Y * py;
            float tcW = tcX + ((float)region.Width * px);
            float tcH = tcY + ((float)region.Height * py);

            int x = (int)position.X;
            int y = (int)position.Y;

            indices = new short[] { 0, 1, 2, 1, 3, 2 };
            vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(x, y, 0), color, new Vector2(tcX, tcY)), //top left
                new VertexPositionColorTexture(new Vector3(x+ size.X, y, 0), color, new Vector2(tcW, tcY)), //top right
                new VertexPositionColorTexture(new Vector3(x, position.Y + size.Y, 0), color, new Vector2(tcX, tcH)), //bottom left
                new VertexPositionColorTexture(new Vector3(x + size.X, y + size.Y, 0), color, new Vector2(tcW, tcH)) //bottom right
            };
        }

#endregion
    }
}
