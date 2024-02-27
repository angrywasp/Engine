using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine;
using Engine.AssetTransport;
using Engine.Content;
using Engine.Content.Model;
using Engine.Graphics.Effects.Builder;
using Engine.World.Objects;
using Microsoft.Xna.Framework.Graphics;
using TextureCubeProcessor.Common;

namespace TextureCubeProcessor.IrradianceMapProcessor
{
    public class Processor
    {
        //Writes a 1/2 resolution irradiance map without mipmaps
        public async Task Process(CommandLine cl, TextureCube envMap, GraphicsDevice graphicsDevice)
        {
            var output = Path.ChangeExtension(cl.Output, "irradiance.texcube");

            Log.Instance.Write($"Compiling irradiance texture cube {Path.GetFileName(output)}");

            Effect effect = null;
            SharedMeshData cube = null;

            await new AsyncUiTask().Run(() => {
                cube = Cube.CreateMeshData(graphicsDevice);

                effect = new EffectBuilder().Start(graphicsDevice)
                .CreateProgram(
                    ShaderBuilder.BuildFromText<VertexShader>(graphicsDevice, ShaderText.VertexShaderCube, null),
                    ShaderBuilder.BuildFromText<PixelShader>(graphicsDevice, IrradianceMapShaderText.PixelShader, null))
                .Finish();
            }).ConfigureAwait(false);

            effect.Parameters["World"].SetValue(Matrix4x4.Identity);
            effect.Parameters["Projection"].SetValue(CubeFace.CreateProjectionMatrix(0.1f, 1));
            effect.Parameters["AlbedoMap"].SetValue(envMap);

            graphicsDevice.SetVertexBuffer(cube.PositionBuffer);
			graphicsDevice.SetIndexBuffer(cube.IndexBuffer);

            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            int sz = envMap.Size / 2;

            RenderTargetCube rt = null;

            await new AsyncUiTask().Run(() => {
                rt = new RenderTargetCube(graphicsDevice, sz, false, SurfaceFormat.Rgba, DepthFormat.Depth32F, 0, RenderTargetUsage.PreserveContents);

                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveX);
                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveY);
                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveZ);
                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeX);
                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeY);
                CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeZ);
            }).ConfigureAwait(false);

            var texCubeData = await TextureCubeWriter.Write(rt).ConfigureAwait(false);

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, texCubeData);
        }
    }
}