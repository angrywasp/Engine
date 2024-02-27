using Engine.Helpers;
using AngryWasp.Logger;
using System;
using Engine.Physics;
using Engine.Content;

namespace EngineScripting
{
    public static class Physics
    {
        internal static string assetPath;

        public static PhysicsModel Create(string file)
        {
            var physicsModel = new PhysicsModel();
            ContentLoader.SaveJson(physicsModel, EngineFolders.ContentPathVirtualToReal(file));
            assetPath = file;
            return physicsModel;
        }

        /// <summary>
        /// Saves the physics model
        /// </summary>
        public static void Save(this PhysicsModel physicsModel)
        {
            ContentLoader.SaveJson(physicsModel, EngineFolders.ContentPathVirtualToReal(assetPath));

            Log.Instance.SetColor(ConsoleColor.DarkCyan);
            Log.Instance.Write($"Saved '{assetPath}'");
            Log.Instance.SetColor(ConsoleColor.White);
        }
    }
}