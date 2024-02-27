using System;
using AngryWasp.Cli.Args;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;
using Engine.Scripting;

namespace C64Emulator
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] rawArgs)
        {
        	var args = Arguments.Parse(rawArgs);
            EngineFolders.Initialize(args.GetString("root"));
			Log.CreateInstance(true);

            Settings.Init("C64Emulator.config");
            ScriptEngine.Initialize(Settings.Engine.ScriptEngine, "C64;EMULATOR", "ScriptEngine.Precompiled.C64Emulator");

            Emulator game = new Emulator();
            game.Run();
        }
    }
}