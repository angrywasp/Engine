namespace Engine.Editor.TerrainEditor.Brushes
{
    public class RadialGradientBrush : TerrainBrush
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/TerrainBrushes/RadialGradient.frag.glsl";

        public RadialGradientBrush(EngineCore engine, int radius) : base(engine, radius) { }
    }
}
