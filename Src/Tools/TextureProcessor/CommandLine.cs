using AngryWasp.Cli.Config;

namespace TextureProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("width", "Texture width. Textures will be resized to this Width x Height. Omit to skip resizing")]
        public int? Width { get; set; }

        [CommandLineArgument("height", "Texture height. Textures will be resized to this Width x Height. Omit to skip resizing")]
        public int? Height { get; set; }

        [CommandLineArgument("input", "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("mipmaps", "Generate mip maps?")]
        public bool GenerateMipMaps { get; set; }
    }
}