using System;
using AngryWasp.Cli.Args;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;

namespace AtariEmulator
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] rawArgs)
        {
        	var args = Arguments.Parse(rawArgs);
            EngineFolders.Initialize(args.GetString("root"));
			Log.CreateInstance(true);

            Settings.Init("AtariEmulator.config");

			//load game specific settings
            string configPath = EngineFolders.SettingsPathVirtualToReal("AtariEmulator.Game.config");

            Emulator game = new Emulator();
            game.Run();
        }
    }
}