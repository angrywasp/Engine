using System.Collections.Generic;
using AngryWasp.Math;
using Microsoft.Xna.Framework;

namespace Engine.Configuration
{
    public enum Input_Method
    {
        KeyboardAndMouse = 0,
        
        OneGamepad = 1,
        TwoGamepads = 2,
        ThreeGamepads = 3,
        FourGamepads = 4,

        OneWiimote = 5,
        TwoWiimotes = 6,
        ThreeWiimotes = 7,
        FourWiimotes = 8,
    }

    public enum TransformSpace
    {
        Local,
        World
    }

    public class InputSettings
    {
        public Input_Method InputMethod { get; set; } = Input_Method.KeyboardAndMouse;
    }

    public class CameraSettings
    {
        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 100.0f;
        public float Fov { get; set; } = 75.0f;
    }

    public class EngineSettings
    {
        public bool FullScreen { get; set; } = false;
        public Vector2i Resolution { get; set; } = new Vector2i(1920, 1080);
        public int ShadowResolution { get; set; } = 4096;
        public int SpotShadowMapCount { get; set; } = 32;
        public bool VerticalSync { get; set; } = true;
        public bool AdaptiveSync { get; set; } = true;
        public int TargetFPS { get; set; } = 0;
        public string TerminalFont { get; set; } = "Engine/Fonts/Default.fontpkg";
        public int TerminalFontSize { get; set; } = 12;
        public int TerminalHeight { get; set; } = 400;
        public InputSettings Input { get; set; } = new InputSettings();
        public ScriptEngineSettings ScriptEngine { get; set; } = new ScriptEngineSettings();
        public NetworkServerSettings ServerHost { get; set; } = new NetworkServerSettings();
        public CameraSettings Camera { get; set; } = new CameraSettings();
        public GizmoSettings EditorGizmo { get; set; } = new GizmoSettings();
        public List<NetworkServerConnectionSettingsItem> SavedServers { get; set; } =
            new List<NetworkServerConnectionSettingsItem> {
                new NetworkServerConnectionSettingsItem()
            };
    }

    public class ScriptEngineSettings
    {
        public List<string> Includes { get; set; } = new List<string>();
        public List<string> RuntimeReferences { get; set; } = new List<string>();
        public List<string> ExternalReferences { get; set; } = new List<string>();
        public List<string> ScriptFiles { get; set; } =
            new List<string> {
                "Scripts/Common/*.cs",
                "Scripts/Console/*.cs"
            };

        public string PrecompiledScriptAssembly { get; set; } = "Engine.Scripting";
    }

    public class NetworkServerSettings
    {
        public int Port { get; set; } = 5555;
        public string PrivateKey { get; set; } = string.Empty;
        public int MaxConnections { get; set; } = 8;
        public string Map { get; set; } = string.Empty;
    }

    public class NetworkServerConnectionSettingsItem
    {
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5555;
        public string Name { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;

        public override string ToString() => $"{Name}-{Address}:{Port}";
    }

    public class GizmoSettings
    {
        public TransformSpace Space { get; set; } = TransformSpace.Local;
        public bool TranslationSnapEnabled { get; set; } = true;
        public bool RotationSnapEnabled { get; set; } = true;
        public bool ScaleSnapEnabled { get; set; } = true;
        public bool PrecisionModeEnabled { get; set; } = false;
        public float TranslationSnapValue { get; set; } = 0.1f;
        public Degree RotationSnapValue { get; set; } = 45.0f;
        public float ScaleSnapValue { get; set; } = 0.1f;
    }
}
