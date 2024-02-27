using Engine.Helpers;
using Engine.Configuration;
using Engine.Scripting;
using AngryWasp.Logger;
using System.Collections.Generic;
using AngryWasp.Cli.Args;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Engine.Game;

namespace MacroProcessor
{
    internal class Program
    {
        private static async Task Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            Log.CreateInstance(true);

            EngineFolders.Initialize(args.Pop().Value);

            string configFile = EngineFolders.SettingsPathVirtualToReal("MacroProcessor.config");

            Settings.Init(configFile);
 
            ScriptEngine.Initialize(Settings.Engine.ScriptEngine, "CONSOLE", "ScriptEngine.Precompiled.MacroProcessor", true);

            ConsoleGame cg = new ConsoleGame();

            var macroFiles = new List<string>();

            foreach (var a in args.All)
                if (string.IsNullOrEmpty(a.Flag))
                    macroFiles.Add(a.Value);

            ScriptEngine.ExecuteMacros(macroFiles);
            cg.ExitGame();
        }
    }
}