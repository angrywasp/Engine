using AngryWasp.Logger;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.Config;
using Engine.Game;

namespace TextureCubeProcessor
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

            new ConsoleGame().Run(async (game) => {
                await new Processor().Process(game, cl).ConfigureAwait(false);
            });
        }
    }
}