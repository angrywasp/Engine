using Engine.Editor.TerrainEditor.Tools;
using Engine.World.Objects;

namespace Engine.Editor.TerrainEditor
{
    public class ToolManager
    {
        private PaintTool paint;
        private EraseTool erase;

        public PaintTool Paint => paint;

        public EraseTool Erase => erase;

        public ToolManager(EngineCore engine, HeightmapTerrain terrain)
        {
            paint = new PaintTool(engine, terrain, 0);
            erase = new EraseTool(engine, terrain);
        }
    }
}
