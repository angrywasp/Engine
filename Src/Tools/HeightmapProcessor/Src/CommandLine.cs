using AngryWasp.Cli.Config;

namespace HeightmapProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("scale", "Vertical scale to apply to the heightmap")]
        public float VerticalScale { get; set; }

        [CommandLineArgument("erode", "Number of times to iterate the erosion algorithm")]
        public int ErosionIterations { get; set; }

        [CommandLineArgument("sediment", "Amount of sediment accumulation in the erosion algorithm")]
        public float SedimentCapacity { get; set; }

        [CommandLineArgument("deposition", "Amount of deposition in the erosion algorithm")]
        public float Deposition { get; set; }

        [CommandLineArgument("softness", "Level of soil softness in the erosion algorithm")]
        public float SoilSoftness { get; set; }

        [CommandLineArgument("smooth", "Number of times to iterate the smoothing algorithm")]
        public int SmoothingIterations { get; set; }
    }
}