using AngryWasp.Logger;
using Engine.AssetTransport;
using Engine.Content.Model.Template;
using Engine.Helpers;
using System;
using System.IO;

namespace EngineScripting
{
    public static class MeshExtensions
    {
        private static string assetPath;

        /// <summary>
        /// Loads a game object at the specified path
        /// </summary>
        /// <param name="path">Thge path to load the game object from</param>
        public static MeshTemplate AsMeshTemplate(this string path)
        {
            assetPath = path;
            return MeshReader.Read(File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(path)));
        }

        /// <summary>
        /// Saves the mesh
        /// </summary>
        public static void Save(this MeshTemplate mesh)
        {
            //todo: add support for skinned meshes
            File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(assetPath), MeshWriter.Write(mesh));
            Log.Instance.SetColor(ConsoleColor.DarkCyan);
            Log.Instance.Write($"Saved '{assetPath}'");
            Log.Instance.SetColor(ConsoleColor.White);
        }

        /// <summary>
        /// Saves a copy of the mesh to a new location
        /// </summary>
        /// <param name="path">The path to save the mesh to</param>
        public static void SaveAs(this MeshTemplate mesh, string path)
        {
            //todo: add support for skinned meshes
            File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(path), MeshWriter.Write(mesh));
            Log.Instance.SetColor(ConsoleColor.DarkCyan);
            Log.Instance.Write($"Saved '{assetPath}'");
            Log.Instance.SetColor(ConsoleColor.White);
        }
    }
}
