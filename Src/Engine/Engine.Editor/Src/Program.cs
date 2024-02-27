using System;
using AngryWasp.Cli.Args;
using AngryWasp.Logger;
using Engine.Configuration;
using Engine.Helpers;
using Engine.Scripting;

namespace Engine.Editor
{
    public class App
    {
        public static string Arg;

        private static EditorGame game;

        public static EditorGame Game => game;

        [STAThread]
        public static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            Arg = args.GetString("asset");
            EngineFolders.Initialize(args.GetString("root"));
			Log.CreateInstance(true);

            Settings.Init("Editor.config");
            ScriptEngine.Initialize(Settings.Engine.ScriptEngine, "EDITOR", "ScriptEngine.Precompiled.Editor");

            game = new EditorGame();
            game.Run();
        }
    }
}
