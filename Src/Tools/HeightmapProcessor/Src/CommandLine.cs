using AngryWasp.Cli.Config;

namespace HeightmapProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", null, "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("scale", 30, "Vertical scale to apply to the heightmap")]
        public float VerticalScale { get; set; }

        [CommandLineArgument("erode", 20, "Number of times to iterate the erosion algorithm")]
        public int ErosionIterations { get; set; }

        [CommandLineArgument("sediment", 0.1f, "Amount of sediment accumulation in the erosion algorithm")]
        public float SedimentCapacity { get; set; }

        [CommandLineArgument("deposition", 0.1f, "Amount of deposition in the erosion algorithm")]
        public float Deposition { get; set; }

        [CommandLineArgument("softness", 0.3f, "Level of soil softness in the erosion algorithm")]
        public float SoilSoftness { get; set; }

        [CommandLineArgument("smooth", 20, "Number of times to iterate the smoothing algorithm")]
        public int SmoothingIterations { get; set; }
    }
}