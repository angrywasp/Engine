namespace Engine.Editor.TerrainEditor.Brushes
{
    public class CircleBrush : TerrainBrush
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/TerrainBrushes/Circle.frag.glsl";

        public CircleBrush(EngineCore engine, int radius) : base(engine, radius) { }
    }
}
