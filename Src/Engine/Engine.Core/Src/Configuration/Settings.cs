using System.IO;
using Newtonsoft.Json;
using AngryWasp.Logger;
using Engine.Content;
using Engine.Helpers;

namespace Engine.Configuration
{
    public static class Settings
    {
        private static string SettingsFile { get; set; }
        public static EngineSettings Engine { get; set; }

        public static void Init(string path)
        {
            if (string.IsNullOrEmpty(path))
                Log.Instance.WriteWarning("No config file defined");

            SettingsFile = EngineFolders.SettingsPathVirtualToReal(path);

            Log.Instance.Write("Loading settings...");

            if (File.Exists(SettingsFile))
                Engine = JsonConvert.DeserializeObject<EngineSettings>(File.ReadAllText(SettingsFile), ContentLoader.DefaultJsonSerializerOptions());
            else
            {
                Engine = new EngineSettings();
                Save();
            }

            Log.Instance.Write("Finished loading settings...");
        }

        public static void Save() => File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(Engine, ContentLoader.DefaultJsonSerializerOptions()));
    }
}
