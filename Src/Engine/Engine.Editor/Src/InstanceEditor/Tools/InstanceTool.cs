using System.Numerics;
using Engine.Debug.Shapes;
using Engine.World;
using Engine.World.Objects;
using Microsoft.Xna.Framework;

namespace Engine.Editor.InstanceEditor.Tools
{
    public abstract class InstanceTool
    {
        protected DebugCircle toolShape;
        protected bool isActive;
        protected EngineCore engine;
        protected HeightmapTerrain terrain;
        protected Map map;

        public DebugCircle ToolShape => toolShape;

        public bool IsActive => isActive;

        public bool AlignToTerrain { get; set; } = false;
        
        public InstanceTool(EngineCore engine, Map map, HeightmapTerrain terrain)
        {
            this.engine = engine;
            this.terrain = terrain;
            this.map = map;
        }

        protected abstract bool Active();

        protected abstract void UpdateTool(Vector3 hitLocation, Vector3 normal);

        public virtual void Update(GameTime gameTime)
        {
            isActive = Active();

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
                UpdateTool(hitLocation, rayHits[0].Normal);

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
    }
}