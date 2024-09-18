using AngryWasp.Cli.Config;

namespace TextureCubeProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("posX", "Positive X texture.")]
        public string PosX { get; set; }

        [CommandLineArgument("negX", "Negative X texture.")]
        public string NegX { get; set; }

        [CommandLineArgument("posY", "Positive Y texture.")]
        public string PosY { get; set; }

        [CommandLineArgument("negY", "Negative Y texture.")]
        public string NegY { get; set; }

        [CommandLineArgument("posZ", "Positive Z texture.")]
        public string PosZ { get; set; }

        [CommandLineArgument("negZ", "Negative Z texture.")]
        public string NegZ { get; set; }

        [CommandLineArgument("mipmaps", "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("irradiance", "Generate irradiance map?")]
        public bool GenerateIrradianceMap { get; set; }

        [CommandLineArgument("reflectance", "Generate reflection map?")]
        public bool GenerateReflectionMap { get; set; }
    }
}