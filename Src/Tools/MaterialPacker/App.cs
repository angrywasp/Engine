using AngryWasp.Logger;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.Config;
using MaterialPacker.Packers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Engine.Graphics.Materials;
using System.IO;
using Engine.AssetTransport;
using Engine.Bitmap.Data;
using AngryWasp.Helpers;

namespace MaterialPacker
{
    /*
    generates all the required textures for a mesh material
    albedo
        rgb: albedo color
          a: alpha mask
    normal
        rgb: normal
    pbr
          r: metalness
          g: ao
          b: roughness
          a: displacement 
    emissive
        rgb: emissive color
          a: emissive factor


    default values

      albedo: (255, 255, 255, 255)
      normal: (128, 128, 255, 255)
         pbr: (0, 255, 255, 0)
    emissive: (0, 0, 0, 255)

    use --exclusions to omit processing of specific textures. Semi-colon delimited list
    options
        albedo;normal;pbr;emissive

    todo: validate input
    */
    public class App
    {
        private static async Task Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            CommandLine cl = new CommandLine();
            if (!ConfigMapper<CommandLine>.Process(args, cl, null))
                return;

            Log.CreateInstance(true);

            List<Task> tasks = new List<Task> {
                Task.Run(() => {
                    if (!cl.Excludes.Contains("albedo"))
                        new AlbedoPacker().Process(cl);
                }),
                Task.Run(() => {
                    if (!cl.Excludes.Contains("normal"))
                        new NormalPacker().Process(cl);
                }),
                Task.Run(() => {
                    if (!cl.Excludes.Contains("pbr"))
                        new PbrPacker().Process(cl);
                }),
                Task.Run(() => {
                    if (!cl.Excludes.Contains("emissive"))
                        new EmissivePacker().Process(cl);
                }),
            };

           await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public static void WriteTexture(Texture_Type type, CommandLine cl, TextureData2D td)
        {
            if (td == null)
                return;
                
            Directory.CreateDirectory(cl.Output);

            string outFile = type == Texture_Type.Default ?
                Path.Combine(cl.Output, $"{cl.Name}.texture").NormalizeFilePath() :
                Path.Combine(cl.Output, $"{cl.Name}_{type.ToString().ToLower()}.texture").NormalizeFilePath();

            File.WriteAllBytes(outFile, Texture2DWriter.Write(cl.GenerateMipMaps ? td.GenerateMipMaps() : new List<TextureData2D> { td }));
        }
    }
}