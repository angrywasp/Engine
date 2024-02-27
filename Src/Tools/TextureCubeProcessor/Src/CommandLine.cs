using AngryWasp.Cli.Config;

namespace TextureCubeProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", null, "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("posX", null, "Positive X texture.")]
        public string PosX { get; set; }

        [CommandLineArgument("negX", null, "Negative X texture.")]
        public string NegX { get; set; }

        [CommandLineArgument("posY", null, "Positive Y texture.")]
        public string PosY { get; set; }

        [CommandLineArgument("negY", null, "Negative Y texture.")]
        public string NegY { get; set; }

        [CommandLineArgument("posZ", null, "Positive Z texture.")]
        public string PosZ { get; set; }

        [CommandLineArgument("negZ", null, "Negative Z texture.")]
        public string NegZ { get; set; }

        [CommandLineArgument("mipmaps", null, "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("irradiance", null, "Generate irradiance map?")]
        public bool GenerateIrradianceMap { get; set; }

        [CommandLineArgument("reflectance", null, "Generate reflection map?")]
        public bool GenerateReflectionMap { get; set; }
    }
}