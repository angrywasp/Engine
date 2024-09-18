using AngryWasp.Cli.Config;

namespace MaterialGenerator
{
    public class CommandLine
    {
        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("albedo", "Path to the albedo texture.")]
        public string Albedo { get; set; }

        [CommandLineArgument("normal", "Path to the normal texture.")]
        public string Normal { get; set; }

        [CommandLineArgument("pbr", "Path to the PBR texture.")]
        public string Pbr { get; set; }

        [CommandLineArgument("emissive", "Path to the emissive texture.")]
        public string Emissive { get; set; }

        [CommandLineArgument("doubleSided", "Make material double sided")]
        public bool DoubleSided { get; set; }
    }
}