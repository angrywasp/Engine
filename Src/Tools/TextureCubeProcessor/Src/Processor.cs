using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine;
using Engine.AssetTransport;
using Engine.Bitmap;
using Engine.Bitmap.Data;
using Engine.Game;
using Microsoft.Xna.Framework.Graphics;
using IProcessor = TextureCubeProcessor.IrradianceMapProcessor.Processor;
using RProcessor = TextureCubeProcessor.ReflectionMapProcessor.Processor;

namespace TextureCubeProcessor
{
    public class Processor
    {
        public async Task Process(ConsoleGame cg, CommandLine cl)
        {
            Log.Instance.Write($"Compiling Texture Cube {Path.GetFileName(cl.Output)} {(cl.GenerateMipMaps ? "with MipMaps" : "without MipMaps")}");

            List<Task<List<(int Width, int Height, byte[] Data)>>> tasks = new List<Task<List<(int Width, int Height, byte[] Data)>>>();

            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.PosX, cl.GenerateMipMaps); }));
            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.NegX, cl.GenerateMipMaps); }));
            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.PosY, cl.GenerateMipMaps); }));
            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.NegY, cl.GenerateMipMaps); }));
            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.PosZ, cl.GenerateMipMaps); }));
            tasks.Add(Task.Run(() => { return ProcessCubeMapFaceBitmap(cl.NegZ, cl.GenerateMipMaps); }));

            await Task.WhenAll(tasks);

            var posX = await tasks[0];
            var negX = await tasks[1];
            var posY = await tasks[2];
            var negY = await tasks[3];
            var posZ = await tasks[4];
            var negZ = await tasks[5];
            
            //todo: don't assume surface format is rgba
            var envMap = await BuildTextureCube(cg.GraphicsDevice, SurfaceFormat.Rgba, posX, negX, posY, negY, posZ, negZ).ConfigureAwait(false);

#pragma warning disable CS4014
            Task.Run(async () =>
            {
                var data = await TextureCubeWriter.Write(envMap).ConfigureAwait(false);
                Directory.CreateDirectory(Path.GetDirectoryName(cl.Output));
                File.WriteAllBytes(cl.Output, data);
            });
#pragma warning restore CS4014

            var processorTasks = new List<Task> {
                Task.Run(async () => {
                    if (cl.GenerateIrradianceMap)
                        await new IProcessor().Process(cl, envMap, cg.GraphicsDevice).ConfigureAwait(false);
                }),
                Task.Run(async () => {
                    if (cl.GenerateReflectionMap)
                        await new RProcessor().Process(cl, envMap, cg.GraphicsDevice).ConfigureAwait(false);
                })
            };

            await Task.WhenAll(processorTasks).ConfigureAwait(false);
            await cg.ExitGameAsync().ConfigureAwait(false);
        }

        private List<(int Width, int Height, byte[] Data)> ProcessCubeMapFaceBitmap(string file, bool mipmaps)
        {
            var t = TextureData2D.FromBitmap(SkiaBitmap.Load(file));
            var interleaved = new List<(int Width, int Height, byte[] Data)>();
            if (mipmaps)
            {
                foreach (var m in t.GenerateMipMaps())
                    interleaved.Add((m.Width, m.Height, m.ToTextureData().Interleave()));

                return interleaved;
            }
            else
                return new List<(int Width, int Height, byte[] Data)> { (t.Width, t.Height, t.ToTextureData().Interleave()) };
        }

        private async Task<TextureCube> BuildTextureCube(GraphicsDevice g, SurfaceFormat format,
            List<(int Width, int Height, byte[] Data)> posX, List<(int Width, int Height, byte[] Data)> negX,
            List<(int Width, int Height, byte[] Data)> posY, List<(int Width, int Height, byte[] Data)> negY,
            List<(int Width, int Height, byte[] Data)> posZ, List<(int Width, int Height, byte[] Data)> negZ)
        {
            TextureCube envMap = null;
            await new AsyncUiTask().Run(() =>
            {
                envMap = new TextureCube(g, posX[0].Width, posX.Count > 1, format);

                for (int i = 0; i < posX.Count; i++)
                {
                    var x = posX[i];
                    envMap.SetData<byte>(CubeMapFace.PositiveX, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }

                for (int i = 0; i < posY.Count; i++)
                {
                    var x = posY[i];
                    envMap.SetData<byte>(CubeMapFace.PositiveY, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }

                for (int i = 0; i < posZ.Count; i++)
                {
                    var x = posZ[i];
                    envMap.SetData<byte>(CubeMapFace.PositiveZ, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }

                for (int i = 0; i < negX.Count; i++)
                {
                    var x = negX[i];
                    envMap.SetData<byte>(CubeMapFace.NegativeX, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }

                for (int i = 0; i < negY.Count; i++)
                {
                    var x = negY[i];
                    envMap.SetData<byte>(CubeMapFace.NegativeY, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }

                for (int i = 0; i < negZ.Count; i++)
                {
                    var x = negZ[i];
                    envMap.SetData<byte>(CubeMapFace.NegativeZ, i, null, x.Data, 0, x.Width * x.Height * format.GetSize());
                }
            }).ConfigureAwait(false);

            return envMap;
        }
    }
}