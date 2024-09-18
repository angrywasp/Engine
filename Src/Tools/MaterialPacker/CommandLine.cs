using AngryWasp.Cli.Config;

namespace MaterialPacker
{
    public class CommandLine
    {
        [CommandLineArgument("width", "Texture width.")]
        public int? Width { get; set; }

        [CommandLineArgument("height", "Texture height.")]
        public int? Height { get; set; }

        [CommandLineArgument("name", "Output file name.")]
        public string Name { get; set; }

        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("albedo", "Path to the albedo texture.")]
        public string Albedo { get; set; }

        [CommandLineArgument("alphaMask", "Path to the alpha mask texture.")]
        public string AlphaMask { get; set; }

        [CommandLineArgument("normal", "Path to the normal texture.")]
        public string Normal { get; set; }

        [CommandLineArgument("pbr", "Path to a pre-computed pbr texture. Use the other PBR texture options to overwrite specific channels")]
        public string Pbr { get; set; }

        [CommandLineArgument("invertNormals", "Invert green channel of normal texture.")]
        public bool InvertNormal { get; set; }

        [CommandLineArgument("emissive", "Path to the emissive texture.")]
        public string Emissive { get; set; }

        [CommandLineArgument("emissiveFactor", "Path to the emissive intensity texture.")]
        public string EmissiveFactor { get; set; }

        [CommandLineArgument("metalness", "Path to the metalness texture")]
        public string Metalness { get; set; }

        [CommandLineArgument("ao", "Path to the ao texture")]
        public string AO { get; set; }

        [CommandLineArgument("roughness", "Path to the roughness texture")]
        public string Roughness { get; set; }

        [CommandLineArgument("displacement", "Path to the displacement texture")]
        public string Displacement { get; set; }

        [CommandLineArgument("mipmaps", "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("exclude", "List of texture types to exclude from processing. options: albedo;normal;pbr;emissive")]
        public string Excludes { get; set; }
    }
}