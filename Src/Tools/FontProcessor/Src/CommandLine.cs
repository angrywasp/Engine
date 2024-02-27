using AngryWasp.Cli.Config;

namespace FontProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", null, "Input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("output", null, "Output file path.")]
        public string Output { get; set; }

        [CommandLineArgument("numbers", false, "Include numbers in the font.")]
        public bool Numbers { get; set; }

        [CommandLineArgument("lower", false, "Include lower case letters in the font.")]
        public bool LowerCase { get; set; }

        [CommandLineArgument("upper", false, "Include upper case letters in the font.")]
        public bool UpperCase { get; set; }

        [CommandLineArgument("symbols", false, "Include printable symbol characters in the font.")]
        public bool Symbols { get; set; }

        [CommandLineArgument("space", 2, "spacing (in pixels) between letters.")]
        public int Spacing { get; set; }

        [CommandLineArgument("sizes", null, "Comma delimited list of font sizes to generate.")]
        public string Sizes { get; set; }
    }
}