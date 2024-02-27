using Microsoft.Xna.Framework;
using System.Numerics;
using Engine.Debug.Shapes;
using Engine.Editor.TerrainEditor.Brushes;
using Engine.World.Objects;

namespace Engine.Editor.TerrainEditor.Tools
{
    public abstract class TerrainTool
    {
        protected EngineCore engine;
        protected HeightmapTerrain terrain;
        protected bool isActive;
        protected TerrainBrush brush;
        protected Color[] brushData1D;
        protected int[,] brushData2D;
        protected Color[] blendTextureData;
        protected int[,] blendTextureColorsArray2D;
        protected int texW, texH;
        protected DebugCircle toolShape;

        public bool IsActive => isActive;
        public DebugCircle ToolShape => toolShape;
        public static readonly int DEFAULT_STRENGTH = 128;
        public int Strength { get; set; } = DEFAULT_STRENGTH;
        public virtual TerrainBrush Brush
        {
            get { return brush; }
            set
            {
                brush = value;
                brush.RadiusChanged += brush_RadiusChanged;
                brush_RadiusChanged(brush, brush.Radius);
            }
        }

        protected virtual void brush_RadiusChanged(TerrainBrush sender, int radius)
        {
            RebuildArrays();
            toolShape.Radius = radius;
        }

        public TerrainTool(EngineCore engine, HeightmapTerrain terrain)
        {
            this.engine = engine;
            this.terrain = terrain;

            texW = terrain.Heightmap.Width;
            texH = terrain.Heightmap.Height;

            blendTextureData = new Color[terrain.BlendTexture.Width * terrain.BlendTexture.Height];
            blendTextureColorsArray2D = new int[terrain.BlendTexture.Width, terrain.BlendTexture.Height];

            int z = 0;
            for (int y = 0; y < texH; y++)
                for (int x = 0; x < texW; x++)
                    blendTextureColorsArray2D[x, y] = z++;

            this.toolShape = new DebugCircle(Vector3.Zero, 0.5f, 64, Color.Red);
        }

        int brushRadius = -1;

        private void RebuildArrays()
        {
            brushRadius = brush.Radius;
            brushData1D = new Color[brush.Mask.Width * brush.Mask.Height];
            brushData2D = new int[brush.Mask.Width, brush.Mask.Height];
            brush.Mask.GetData<Color>(brushData1D);

            int z = 0;
            for (int x = 0; x < brush.Mask.Width; x++)
                for (int y = 0; y < brush.Mask.Height; y++)
                    brushData2D[x, y] = z++;
        }

        private int[] GetArrayRegion(int[,] a, Rectangle r)
        {
            int[] i = new int[r.Width * r.Height];

            int startX = r.X;
            int endX = r.X + r.Width;

            int startY = r.Y;
            int endY = r.Y + r.Height;

            int z = 0;

            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    i[z++] = a[x, y];

            return i;
        }


        public virtual void Update(GameTime gameTime)
        {
            isActive = true;

            if (brush == null)
            {
                isActive = false;
                return;
            }

            if (!engine.Input.Mouse.ButtonDown(engine.Input.Mouse.RightButton))
                isActive = false;

            Ray r = engine.Input.Mouse.ToRay(engine.Camera.View, engine.Camera.Projection);
            var rayHits = engine.Scene.Physics.RayCast(r);

            if (rayHits.Count == 0)
            {
                isActive = false;
                return;
            }

            Vector3 hitLocation;
            r.GetPointOnRay(rayHits[0].T, out hitLocation);

            if (isActive)
                UpdateTool((int)hitLocation.X, (int)hitLocation.Z);

            toolShape.Position = hitLocation;
            engine.Interface.ScreenMessages.WriteStaticText(1, toolShape.Position.ToString(), Color.Aquamarine);

            float[] heights = new float[toolShape.Vertices.Length];

            for (int i = 0; i < toolShape.Vertices.Length; i++)
            {
                var p = toolShape.Vertices[i].Position + toolShape.Position;

                r = new Ray(new Vector3(p.X, 1000, p.Z), -Vector3.UnitY, 1000);
                rayHits = engine.Scene.Physics.RayCast(r);
                if (rayHits.Count == 0)
                    heights[i] = 0;
                else
                {
                    r.GetPointOnRay(rayHits[0].T, out hitLocation);
                    heights[i] = (hitLocation.Y - toolShape.Position.Y) + 0.025f;
                }
            }

            toolShape.DeformHeights(heights);
        }

        protected virtual void UpdateTool(int x, int z)
        {
            int dia = brush.Radius * 2;
            int minX = x - brush.Radius;
            int maxX = x + brush.Radius;
            int minZ = z - brush.Radius;
            int maxZ = z + brush.Radius;

            if (minX < 0)
                minX = 0;

            if (maxX >= texH)
                maxX = texH;

            if (minZ < 0)
                minZ = 0;

            if (maxZ >= texW)
                maxZ = texW;

            Rectangle tRect = new Rectangle(minX, minZ, maxX - minX, maxZ - minZ);
            //the clipped area of the mask to use
            Rectangle cRect = new Rectangle(dia - tRect.Width, dia - tRect.Height,
                dia - (dia - tRect.Width), dia - (dia - tRect.Height));

            if (maxX >= texH)
                cRect.X = 0;

            if (maxZ >= texW)
                cRect.Y = 0;

            int[] m = GetArrayRegion(brushData2D, cRect);
            int[] t = GetArrayRegion(blendTextureColorsArray2D, tRect);

            terrain.BlendTexture.GetData<Color>(blendTextureData);

            for (int i = 0; i < m.Length; i++)
                UpdatePixel(brushData1D[m[i]], ref blendTextureData[t[i]]);

            terrain.BlendTexture.SetData<Color>(blendTextureData);
            terrain.Material.ApplyBlendTexture(terrain.BlendTexture);
        }

        public virtual void Draw() { }

        protected abstract void UpdatePixel(Color mask, ref Color blendTex);
    }
}