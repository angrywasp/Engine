using System;
using AngryWasp.Logger;
using Engine.Helpers;
using System.IO;
using Engine.Content;
using Newtonsoft.Json;
using Engine.World;
using Engine.World.Components;
using Engine.World.Components.Lights;

namespace EngineScripting
{
    public static class Entity
    {
        private static string assetPath;
        /// <summary>
        /// Loads a game object at the specified path
        /// </summary>
        /// <param name="path">Thge path to load the game object from</param>
        public static GameObjectType Load<T>(string path) where T : GameObjectType
        {
            assetPath = path;
            string jsonString = File.ReadAllText(EngineFolders.ContentPathVirtualToReal(path));
            return JsonConvert.DeserializeObject<T>(jsonString, ContentLoader.DefaultJsonSerializerOptions());
        }

        /// <summary>
        /// Creates a new .type file based on the type T
        /// </summary>
        /// <param name="file">The file path of the new .type file</param>
        /// <remarks>
        /// Types that can be created must be based on GameObjectType.
        /// File path is relative to the content directory
        /// </remarks>
        public static T Create<T>(string file) where T : GameObjectType, new()
        {
            //todo: createdirectory if it doesn't exist
            T t = new T();
            string fullPath = EngineFolders.ContentPathVirtualToReal(file);

            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            ContentLoader.SaveJson(t, fullPath);

            assetPath = file;
            return t;
        }

        /// <summary>
        /// Adds a mesh component to the game object
        /// </summary>
        /// <param name="path">The path to the mesh to add</param>
        /// <param name="name">The name of the new component</param>
        public static MeshComponentType AddMeshComponent(this GameObjectType gameObjectType, string path, string name)
        {
            MeshComponentType ct = AddComponent<MeshComponentType>(ref gameObjectType, name);
            ct.Mesh = path;
            return ct;
        }

        /// <summary>
        /// Adds an emitter component to the game object
        /// </summary>
        /// <param name="name">The name of the new component</param>
        public static EmitterComponentType AddEmitterComponent(this GameObjectType gameObjectType, string name)
        {
            EmitterComponentType ct = AddComponent<EmitterComponentType>(ref gameObjectType, name);
            return ct;
        }

        /// <summary>
        /// Adds a directional light component to the game object
        /// </summary>
        /// <param name="name">The name of the new component</param>
        public static DirectionalLightComponentType AddDirectionalLightComponent(this GameObjectType gameObjectType, string name)
        {
            DirectionalLightComponentType ct = AddComponent<DirectionalLightComponentType>(ref gameObjectType, name);
            return ct;
        }

        /// <summary>
        /// Adds a spot light component to the game object
        /// </summary>
        /// <param name="name">The name of the new component</param>
        public static SpotLightComponentType AddSpotLightComponent(this GameObjectType gameObjectType, string name)
        {
            SpotLightComponentType ct = AddComponent<SpotLightComponentType>(ref gameObjectType, name);
            return ct;
        }

        /// <summary>
        /// Adds a point light component to the game object
        /// </summary>
        /// <param name="name">The name of the new component</param>
        public static PointLightComponentType AddPointLightComponent(this GameObjectType gameObjectType, string name)
        {
            PointLightComponentType ct = AddComponent<PointLightComponentType>(ref gameObjectType, name);
            return ct;
        }

        public static GameObjectComponentType AddGameObjectComponent(this GameObjectType gameObjectType, string path, string name)
        {
            GameObjectComponentType ct = AddComponent<GameObjectComponentType>(ref gameObjectType, name);
            ct.GameObjectPath = path;
            return ct;
        }

        /// <summary>
        /// Saves the game object
        /// </summary>
        public static void Save(this GameObjectType gameObjectType)
        {
            ContentLoader.SaveJson(gameObjectType, EngineFolders.ContentPathVirtualToReal(assetPath));

            Log.Instance.SetColor(ConsoleColor.DarkCyan);
            Log.Instance.Write($"Saved '{assetPath}'");
            Log.Instance.SetColor(ConsoleColor.White);
        }

        private static T AddComponent<T>(ref GameObjectType gameObjectType, string name) where T : ComponentType, new()
        {
            T ct = new T();
            ct.Name = name;

            gameObjectType.Components.Add(ct);

            return ct;
        }
    }
}