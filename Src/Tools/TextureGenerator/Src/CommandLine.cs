using AngryWasp.Cli.Config;

namespace TextureGenerator
{
    public class CommandLine
    {
        [CommandLineArgument("output", null, "Output directory for generated files.")]
        public string Output { get; set; }
    }
}