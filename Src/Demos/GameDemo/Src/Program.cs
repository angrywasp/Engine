using System;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;
using Engine.Scripting;
using AngryWasp.Cli.Args;

namespace GameDemo
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            EngineFolders.Initialize(args.GetString("root"));
			Log.CreateInstance(true);

            Settings.Init("DemoGame.config");
            ScriptEngine.Initialize(Settings.Engine.ScriptEngine, null, "ScriptEngine.Precompiled.GameDemo");

			new BasicGame().Run();
        }
    }
}
