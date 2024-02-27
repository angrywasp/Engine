using System;
using System.IO;
using System.Runtime.InteropServices;
using AngryWasp.Cli.Args;
using AngryWasp.Cli.Config;
using AngryWasp.Logger;
using Engine.AssetTransport;
using Engine.Helpers;
using MeshProcessor.Materials;
using MeshProcessor.Model;
using MeshProcessor.Skeleton;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;

namespace MeshProcessor
{
    internal class Program
    {
        private static void Main(string[] rawArgs)
        {
            var args = Arguments.Parse(rawArgs);
            CommandLine cl = new CommandLine();
            if (!ConfigMapper<CommandLine>.Process(args, cl, null))
                return;

            Log.CreateInstance(true);
            EngineFolders.Initialize(cl.Root);

            string fileName = Path.GetFileNameWithoutExtension(cl.Input);

            if (cl.Flip)
                fileName += "Inverted";

            fileName += cl.Collision ? ".collision" : ".mesh";
            string filePath = Path.Combine(cl.MeshOutput, fileName);

            Log.Instance.Write($"Compiling Mesh {filePath}");

            ModelRoot model = null;

            try
            {
                model = ModelRoot.Load(cl.Input, ValidationMode.TryFix);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError($"Model loading failed.\n{ex.Message}\nTrying again without validation");

                try
                {
                    model = ModelRoot.Load(cl.Input, ValidationMode.Skip);
                }
                catch
                {
                    Log.Instance.WriteError($"The model cannot be imported");
                }
            }

            if (model == null)
                return;

            if (!string.IsNullOrEmpty(cl.MeshOutput))
                Directory.CreateDirectory(EngineFolders.ContentPathVirtualToReal(cl.MeshOutput));

            if (cl.Collision)
            {
                var meshFactory = new CollisionMeshFactory(model, cl.Flip);
                File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(filePath), MemoryMarshal.AsBytes<Triangle>(meshFactory.Triangles).ToArray());
            }
            else
            {
                if (!string.IsNullOrEmpty(cl.MaterialOutput))
                    Directory.CreateDirectory(EngineFolders.ContentPathVirtualToReal(cl.MaterialOutput));
                
                if (!string.IsNullOrEmpty(cl.TextureOutput))
                    Directory.CreateDirectory(EngineFolders.ContentPathVirtualToReal(cl.TextureOutput));


                var skelFactory = new SkeletonFactory(model);
                var matFactory = new MaterialProcessor(model, cl);
                
                if (!string.IsNullOrEmpty(cl.MeshOutput))
                {
                    var meshFactory = new MeshFactory(model, skelFactory.Skeleton, skelFactory.Skins, matFactory.Materials, cl.Flip, cl.RecalculateNormals);
                    File.WriteAllBytes(EngineFolders.ContentPathVirtualToReal(filePath), MeshWriter.Write(meshFactory.Mesh));
                }
            }
        }
    }
}
