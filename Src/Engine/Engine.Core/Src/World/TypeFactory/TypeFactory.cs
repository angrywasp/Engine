using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Engine.Helpers;
using AngryWasp.Logger;
using AngryWasp.Helpers;
using Engine.Content;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Engine.World
{
    public class TypeFactory
    {
        private EngineCore engine;
        private Dictionary<string, TypeFactoryItem> items = new Dictionary<string, TypeFactoryItem>();

        public Dictionary<string, TypeFactoryItem> Items => items;

        private Dictionary<string, (Type, Type)> classTypePairs = new Dictionary<string, (Type, Type)>();

        public TypeFactory(EngineCore e)
        {
            engine = e;
            items.Clear();

            Assembly ass = ReflectionHelper.Instance.LoadAssemblyFile(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Engine.Core.dll"));

            foreach (var t in ReflectionHelper.Instance.AssemblyTypeCache.Values)
                ExtractTypes(t);

            string[] files = Directory.GetFiles(EngineFolders.ContentPath, "*.type", SearchOption.AllDirectories);

            foreach (string file in files)
                CacheTypeFile(file);
        }

        public void CacheTypeFile(string file)
        {
            string virtualFile = EngineFolders.ContentPathRealToVirtual(file);
            if (items.ContainsKey(virtualFile))
                return;

            if (!File.Exists(file))
            {
                Log.Instance.WriteFatal($"Asset missing: {file}");
                return;
            }

            string jsonString = File.ReadAllText(file);

            var deserialized = JsonConvert.DeserializeObject(jsonString, ContentLoader.DefaultJsonSerializerOptions());
            var got = ((GameObjectType)deserialized);
            got.Load(engine);

            string key = got.GetType().FullName;

            Type classType = classTypePairs[key].Item1;
            Type typeType = classTypePairs[key].Item2;

            items.Add(virtualFile, new TypeFactoryItem(got, typeType, classType));
        }

        public async Task<T> CreateGameObjectAsync<T>(string file) where T : GameObject
        {
            if (!items.ContainsKey(file))
            {
                Log.Instance.WriteFatal($"TypeFactory does not contain '{file}'");
                return null;
            }

            TypeFactoryItem i = items[file];
            T go = (T)Activator.CreateInstance(i.ClassType);
            SetTypeFields(i.ClassType, go, i.TypeInstance);
            await go.LoadAsync(engine).ConfigureAwait(false);
            return go;
        }

        private void ExtractTypes(Type t)
        {
            if (ReflectionHelper.Instance.InheritsOrImplements(t, typeof(GameObject)))
            {
                FieldInfo pi = t.GetField("_type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                Type classType = pi.DeclaringType;
                Type typeType = pi.FieldType;
                classTypePairs.Add(typeType.FullName, (classType, typeType));
            }
        }

        public void SetTypeFields(Type type, object instance, object typeInstance)
        {
            FieldInfo fi = type.GetField("_type", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi == null)
                Log.Instance.WriteFatalException(new Exception("_type property doesn't exist"));

            fi.SetValue(instance, typeInstance);

            Type baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                baseType.GetField("_type", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, typeInstance);
                baseType = baseType.BaseType;
            }
        }
    }
}
