using AngryWasp.Cli.Config;

namespace MaterialPacker
{
    public class CommandLine
    {
        [CommandLineArgument("width", null, "Texture width.")]
        public int? Width { get; set; }

        [CommandLineArgument("height", null, "Texture height.")]
        public int? Height { get; set; }

        [CommandLineArgument("name", "Texture", "Output file name.")]
        public string Name { get; set; }

        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("albedo", null, "Path to the albedo texture.")]
        public string Albedo { get; set; }

        [CommandLineArgument("alphaMask", null, "Path to the alpha mask texture.")]
        public string AlphaMask { get; set; }

        [CommandLineArgument("normal", null, "Path to the normal texture.")]
        public string Normal { get; set; }

        [CommandLineArgument("pbr", null, "Path to a pre-computed pbr texture. Use the other PBR texture options to overwrite specific channels")]
        public string Pbr { get; set; }

        [CommandLineArgument("invertNormals", null, "Invert green channel of normal texture.")]
        public bool InvertNormal { get; set; }

        [CommandLineArgument("emissive", null, "Path to the emissive texture.")]
        public string Emissive { get; set; }

        [CommandLineArgument("emissiveFactor", null, "Path to the emissive intensity texture.")]
        public string EmissiveFactor { get; set; }

        [CommandLineArgument("metalness", null, "Path to the metalness texture")]
        public string Metalness { get; set; }

        [CommandLineArgument("ao", null, "Path to the ao texture")]
        public string AO { get; set; }

        [CommandLineArgument("roughness", null, "Path to the roughness texture")]
        public string Roughness { get; set; }

        [CommandLineArgument("displacement", null, "Path to the displacement texture")]
        public string Displacement { get; set; }

        [CommandLineArgument("mipmaps", null, "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("exclude", "", "List of texture types to exclude from processing. options: albedo;normal;pbr;emissive")]
        public string Excludes { get; set; }
    }
}