using AngryWasp.Logger;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.Config;

namespace TextureProcessor
{
    internal class Program
    {
        private static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            CommandLine cl = new CommandLine();
            if (!ConfigMapper<CommandLine>.Process(args, cl, null))
                return;

            Log.CreateInstance(true);

            new Processor().Process(cl);
        }
    }
}