using System.IO;
using System.Numerics;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.Config;
using AngryWasp.Logger;
using Engine.AssetTransport;

namespace MaterialGenerator
{
    //todo: need to pass texture scale in through the command line
    class MainClass
	{
		public static void Main(string[] rawArgs)
		{
            var args = Arguments.Parse(rawArgs);
            CommandLine cl = new CommandLine();
            if (!ConfigMapper<CommandLine>.Process(args, cl, null))
                return;
                
			Log.CreateInstance(true);

			Log.Instance.Write($"Generating Material {cl.Output}");

            var data = MeshMaterialWriter.Write(cl.Albedo, cl.Normal, cl.Pbr, cl.Emissive, Vector2.One, cl.DoubleSided);

            Directory.CreateDirectory(Path.GetDirectoryName(cl.Output));
            File.WriteAllBytes(cl.Output, data);
		}
	}
}
