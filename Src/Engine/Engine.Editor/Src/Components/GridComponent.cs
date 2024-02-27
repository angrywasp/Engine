using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;
using System.Numerics;
using System.Threading.Tasks;
using Engine.Graphics.Effects;
using Engine.Graphics.Vertices;

namespace Engine.Editor.Components
{
    public sealed class GridComponent
    {
        private float spacing;
        private int gridSize = 25;
		private Color lineColor = Color.Black;
		private Color highlightColor = Color.Red;
		private int lineCount;
		private ColorEffect effect;
        private EngineCore engine;
		private VertexPositionColor[] vertexData;
		private Matrix4x4 worldMatrix;

        private float y;

        public GridComponent(EngineCore engine, float y, float gridspacing)
        {
            effect = new ColorEffect();
            this.engine = engine;
            spacing = gridspacing;
            this.y = y;
        }

        public async Task LoadAsync()
        {
            await effect.LoadAsync(engine.GraphicsDevice).ConfigureAwait(false);
            worldMatrix = Matrix4x4.CreateTranslation(0, y, 0);
            effect.World = worldMatrix;
            effect.ColorMultiplier = new Vector4(1, 1, 1, 0.25f);
            ResetLines();
        }

        public void ResetLines()
        {
            lineCount = (((int)(gridSize / spacing) * 4) + 2) * 2;
            vertexData = new VertexPositionColor[lineCount];

            int x = 0;
            // fill array
            for (int i = 1; i < (gridSize / spacing) + 1; i++)
            {
                vertexData[x++] = new VertexPositionColor(new Vector3((i * spacing), 0, gridSize), lineColor);
                vertexData[x++] = new VertexPositionColor(new Vector3((i * spacing), 0, -gridSize), lineColor);

                vertexData[x++] = new VertexPositionColor(new Vector3((-i * spacing), 0, gridSize), lineColor);
                vertexData[x++] = new VertexPositionColor(new Vector3((-i * spacing), 0, -gridSize), lineColor);

                vertexData[x++] = new VertexPositionColor(new Vector3(gridSize, 0, (i * spacing)), lineColor);
                vertexData[x++] = new VertexPositionColor(new Vector3(-gridSize, 0, (i * spacing)), lineColor);

                vertexData[x++] = new VertexPositionColor(new Vector3(gridSize, 0, (-i * spacing)), lineColor);
                vertexData[x++] = new VertexPositionColor(new Vector3(-gridSize, 0, (-i * spacing)), lineColor);
            }

            // add highlights
            vertexData[x++] = new VertexPositionColor(Vector3Orientation.Forward * gridSize, highlightColor);
            vertexData[x++] = new VertexPositionColor(Vector3Orientation.Backward * gridSize, highlightColor);

            vertexData[x++] = new VertexPositionColor(Vector3Orientation.Right * gridSize, highlightColor);
            vertexData[x++] = new VertexPositionColor(Vector3Orientation.Left * gridSize, highlightColor);
        }

        public void Draw()
        {
			engine.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            engine.GraphicsDevice.BlendState = BlendState.Opaque;

            effect.View = engine.Camera.View;
            effect.Projection = engine.Camera.Projection;
            
			effect.Apply();
			engine.GraphicsDevice.DrawUserPrimitives(GLPrimitiveType.Lines, vertexData, 0, lineCount, VertexPositionColor.VertexDeclaration);
        }
    }
}
