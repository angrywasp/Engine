using AngryWasp.Cli.Config;

namespace MeshProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("root", "Root engine content directory.")]
        public string Root { get; set; }

        [CommandLineArgument("meshOutput", "Relative output directory for exported mesh files.")]
        public string MeshOutput { get; set; }

        [CommandLineArgument("materialOutput", "Relative output directory for exported material files.")]
        public string MaterialOutput { get; set; }

        [CommandLineArgument("textureOutput", "Relative output directory for exported texture files.")]
        public string TextureOutput { get; set; }

        [CommandLineArgument("flip", "Flip the mesh faces")]
        public bool Flip { get; set; }

        [CommandLineArgument("mipmaps", "Generate mip maps for exported textures?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("recalculate", "Recalculate normals?")]
        public bool RecalculateNormals { get; set; }

        [CommandLineArgument("invertNormals", "Invert green channel of exported normal textures.")]
        public bool InvertNormals { get; set; }

        [CommandLineArgument("collision", "Exports the mesh as a collision mesh.")]
        public bool Collision { get; set; }
    }
}
