using System;
using AngryWasp.Cli.Args;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;
using Engine.Scripting;

namespace NesEmulator
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] rawArgs)
        {
        	var args = Arguments.Parse(rawArgs);
			EngineFolders.Initialize(args.GetString("root"));
			Log.CreateInstance(true);

            Settings.Init("NesEmulator.config");
            ScriptEngine.Initialize(Settings.Engine.ScriptEngine, "NES;EMULATOR", "ScriptEngine.Precompiled.NesEmulator");

			//load game specific settings
            string configPath = EngineFolders.SettingsPathVirtualToReal("NesEmulator.Game.config");
            
            Emulator game = new Emulator();
            game.Run();
        }
    }
}