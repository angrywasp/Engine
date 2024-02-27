using AngryWasp.Cli.Config;

namespace MaterialGenerator
{
    public class CommandLine
    {
        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("albedo", "Engine/Textures/Default_albedo.texture", "Path to the albedo texture.")]
        public string Albedo { get; set; }

        [CommandLineArgument("normal", "Engine/Textures/Default_normal.texture", "Path to the normal texture.")]
        public string Normal { get; set; }

        [CommandLineArgument("pbr", "Engine/Textures/Default_pbr.texture", "Path to the PBR texture.")]
        public string Pbr { get; set; }

        [CommandLineArgument("emissive", "Engine/Textures/Default_emissive.texture", "Path to the emissive texture.")]
        public string Emissive { get; set; }

        [CommandLineArgument("doubleSided", null, "Make material double sided")]
        public bool DoubleSided { get; set; }
    }
}