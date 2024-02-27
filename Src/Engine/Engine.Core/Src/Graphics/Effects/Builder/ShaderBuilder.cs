using System;
using System.IO;
using System.Text;
using AngryWasp.Logger;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Engine.Graphics.Effects.Builder
{
    public class ShaderBuilder
    {
        public static T BuildFromFile<T>(GraphicsDevice device, string filePath, string[] includeDirs) where T : Shader, new()
        {
            var meta = new ShaderMetadata();
            meta.ProgramPath = EngineFolders.ContentPathVirtualToReal(filePath);

            var lines = File.ReadAllLines(meta.ProgramPath);

            ParseShaderMetadata(ref meta, lines, includeDirs, out int lineOffset);

            return Build<T>(device, meta, lines, lineOffset);
        }

        public static T BuildFromText<T>(GraphicsDevice device, string shaderText, string[] includeDirs) where T : Shader, new()
        {
            var meta = new ShaderMetadata();
            meta.ProgramPath = "Dynamic text";

            var lines = shaderText.Split(Environment.NewLine);

            ParseShaderMetadata(ref meta, lines, includeDirs, out int lineOffset);

            return Build<T>(device, meta, lines, lineOffset);
        }

        private static T Build<T>(GraphicsDevice device, ShaderMetadata metadata, string[] lines, int lineOffset) where T : Shader, new()
        {
            StringBuilder sb = new StringBuilder();

            int glslVersion = 0;

            try
            {
                var version = GL.GetString(StringName.ShaderLanguageVersion).Split(' ')[0];
                glslVersion = (int)(float.Parse(version) * 100.0f);
            }
            catch
            {
                throw new ArgumentException("Could not obtain GLSL version");
            }

            sb.AppendLine($"#version {glslVersion}");

            for (int i = lineOffset; i < lines.Length; i++)
            {
                var s = lines[i];

                if (s.StartsWith("#include"))
                {
                    string includeFile = s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                    if (!metadata.Includes.ContainsKey(includeFile))
                        throw new Exception($"File contains #include {includeFile} which is not present in the metadata");
                    sb.AppendLine(metadata.Includes[includeFile]);
                }
                else
                    sb.AppendLine(s);
            }

            string shaderText = sb.ToString();

            T shader = new T();
            shader.SetMetadata(metadata);

            string log;
            bool compiled = shader.Compile(device, shaderText, out log);

            if (!compiled)
                Log.Instance.WriteFatal($"Shader compilation failed.\n{FormatLineNumbers(shaderText)}\n{metadata.ProgramPath}\n{log}");
            else if (!string.IsNullOrEmpty(log))
                Log.Instance.WriteWarning($"Shader compiled successfully, but returned warnings.\n{metadata.ProgramPath}\n{log}");

            return shader;
        }

        private static string FormatLineNumbers(string shaderText)
        {
            var sb = new StringBuilder();
            string[] lines = shaderText.Split(Environment.NewLine);

            var pad = 1;
            if (lines.Length >= 100)
                pad = 3;
            else if (lines.Length >= 10)
                pad = 2;

            for (int i = 0; i < lines.Length; i++)
                sb.AppendLine($"{(i + 1).ToString().PadLeft(pad)}: {lines[i]}");

            return sb.ToString();
        }

        public static void ParseShaderMetadata(ref ShaderMetadata meta, string[] fileLines, string[] includeDirs, out int lineOffset)
        {
#if DEBUG
            try
            {
#endif
                lineOffset = 0;

                foreach (var fileLine in fileLines)
                {
                    if (string.IsNullOrWhiteSpace(fileLine))
                        break;

                    var line = fileLine.Trim();

                    if (line.Trim().StartsWith("#include"))
                    {
                        // #include <key> <value>
                        var split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        string includeFileText = null;
                        if (includeDirs == null)
                            throw new ArgumentException($"Shader includes an #include directive and no include directories");

                        foreach (var i in includeDirs)
                        {
                            string includeFilePath = Path.Combine(i, split[2].Trim());

                            if (!File.Exists(includeFilePath))
                                continue;

                            includeFileText = File.ReadAllText(includeFilePath);
                            break;
                        }

                        if (includeFileText == null)
                            throw new FileNotFoundException($"Could not file include file {split[2].Trim()}");

                        meta.Includes.Add(split[1].Trim(), includeFileText);
                    }

                    if (line.StartsWith("#attribute"))
                    {
                        // #attribute <name> <usage> <index>
                        var split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        var name = split[1].Trim();
                        var usage = Enum.Parse<VertexElementUsage>(split[2].Trim(), true);
                        var usageIndex = int.Parse(split[3].Trim());
                        meta.Layout.Add(new VertexElementMetadata(name, usage, meta.Layout.Count, usageIndex));
                    }

                    if (line.StartsWith("#sampler"))
                    {
                        // #sampler <name> <TextureFilter> <TextureAddressMode> 
                        // then override u, v or w with(u=<TextureAddressMode> v=<TextureAddressMode> w=<TextureAddressMode>)
                        var split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        var name = split[1].Trim();
                        var filter = Enum.Parse<TextureFilter>(split[2].Trim(), true);
                        var mode = Enum.Parse<TextureAddressMode>(split[3].Trim(), true);

                        TextureAddressMode u = mode, v = mode, w = mode;

                        if (split.Length > 4)
                            for (int i = 4; i < split.Length; i++)
                            {
                                string[] split2 = split[i].Split("=", StringSplitOptions.RemoveEmptyEntries);
                                switch (split2[0].ToLower())
                                {
                                    case "u": u = Enum.Parse<TextureAddressMode>(split2[1].Trim(), true); break;
                                    case "v": v = Enum.Parse<TextureAddressMode>(split2[1].Trim(), true); break;
                                    case "w": w = Enum.Parse<TextureAddressMode>(split2[1].Trim(), true); break;
                                }
                            }

                        meta.SamplerStates.Add(new SamplerStateMetadata(name, filter, u, v, w));
                    }

                    ++lineOffset;
                }
#if DEBUG
            }
            catch (Exception ex)
            {
                throw Log.Instance.WriteFatalException(ex, meta.ProgramPath);
            }
#endif
        }
    }
}