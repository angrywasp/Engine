using AngryWasp.Cli.Config;

namespace FontProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", "Input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("numbers", "Include numbers in the font.")]
        public bool Numbers { get; set; }

        [CommandLineArgument("lower", "Include lower case letters in the font.")]
        public bool LowerCase { get; set; }

        [CommandLineArgument("upper", "Include upper case letters in the font.")]
        public bool UpperCase { get; set; }

        [CommandLineArgument("symbols", "Include printable symbol characters in the font.")]
        public bool Symbols { get; set; }

        [CommandLineArgument("space", "spacing (in pixels) between letters.")]
        public int Spacing { get; set; }

        [CommandLineArgument("sizes", "Comma delimited list of font sizes to generate.")]
        public string Sizes { get; set; }
    }
}