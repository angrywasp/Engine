using Engine.Editor.TerrainEditor.Brushes;
using System.Threading.Tasks;

namespace Engine.Editor.TerrainEditor
{
    public class BrushManager
    {
        public static readonly int DEFAULT_RADIUS = 16;
        private CircleBrush circle;
        private SquareBrush square;
        private RadialGradientBrush radialGradient;

        public CircleBrush Circle => circle;
        
        public SquareBrush Square => square;

        public RadialGradientBrush RadialGradient => radialGradient;

        public BrushManager(EngineCore engine)
        {
            circle = new CircleBrush(engine, DEFAULT_RADIUS);
            square = new SquareBrush(engine, DEFAULT_RADIUS);
            radialGradient = new RadialGradientBrush(engine, DEFAULT_RADIUS);
        }

        public async Task LoadAsync()
        {
            await Task.WhenAll(
                circle.LoadAsync(),
                square.LoadAsync(),
                radialGradient.LoadAsync()).ConfigureAwait(false);
        }
    }
}
