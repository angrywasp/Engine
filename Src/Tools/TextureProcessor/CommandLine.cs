using AngryWasp.Cli.Config;

namespace TextureProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("width", null, "Texture width. Textures will be resized to this Width x Height. Omit to skip resizing")]
        public int? Width { get; set; }

        [CommandLineArgument("height", null, "Texture height. Textures will be resized to this Width x Height. Omit to skip resizing")]
        public int? Height { get; set; }

        [CommandLineArgument("input", null, "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("mipmaps", null, "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }
    }
}