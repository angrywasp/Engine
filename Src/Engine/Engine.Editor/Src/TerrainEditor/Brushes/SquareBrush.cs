namespace Engine.Editor.TerrainEditor.Brushes
{
    public class SquareBrush : TerrainBrush
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/TerrainBrushes/Square.frag.glsl";

        public SquareBrush(EngineCore engine, int radius) : base(engine, radius) { }
    }
}
