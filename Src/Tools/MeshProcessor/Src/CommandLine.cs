using AngryWasp.Cli.Config;

namespace MeshProcessor
{
    public class CommandLine
    {
        [CommandLineArgument("input", null, "input file path.")]
        public string Input { get; set; }

        [CommandLineArgument("root", null, "Root engine content directory.")]
        public string Root { get; set; }

        [CommandLineArgument("meshOutput", null, "Relative output directory for exported mesh files.")]
        public string MeshOutput { get; set; }

        [CommandLineArgument("materialOutput", null, "Relative output directory for exported material files.")]
        public string MaterialOutput { get; set; }

        [CommandLineArgument("textureOutput", null, "Relative output directory for exported texture files.")]
        public string TextureOutput { get; set; }

        [CommandLineArgument("flip", null, "Flip the mesh faces")]
        public bool Flip { get; set; }

        [CommandLineArgument("mipmaps", null, "Generate mip maps for exported textures?")]
        public bool GenerateMipMaps { get; set; }

        [CommandLineArgument("recalculate", null, "Recalculate normals?")]
        public bool RecalculateNormals { get; set; }

        [CommandLineArgument("invertNormals", null, "Invert green channel of exported normal textures.")]
        public bool InvertNormals { get; set; }

        [CommandLineArgument("collision", null, "Exports the mesh as a collision mesh.")]
        public bool Collision { get; set; }
    }
}
