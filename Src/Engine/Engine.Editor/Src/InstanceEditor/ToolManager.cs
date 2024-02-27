using Engine.Editor.InstanceEditor.Tools;
using Engine.World;
using Engine.World.Objects;

namespace Engine.Editor.InstanceEditor
{
    public class ToolManager
    {
        private PaintTool paint;
        private PlaceTool place;

        public PaintTool Paint => paint;

        public PlaceTool Place => place;

        public ToolManager(EngineCore engine, Map map, HeightmapTerrain terrain)
        {
            paint = new PaintTool(engine, map, terrain);
            place = new PlaceTool(engine, map, terrain);
        }
    }
}
