using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine;
using Engine.AssetTransport;
using Engine.Bitmap.Data;
using Engine.Content;
using Engine.Content.Model;
using Engine.Graphics.Effects.Builder;
using Engine.World.Objects;
using Microsoft.Xna.Framework.Graphics;
using TextureCubeProcessor.Common;

namespace TextureCubeProcessor.ReflectionMapProcessor
{
    public class Processor
    {
        public async Task Process(CommandLine cl, TextureCube envMap, GraphicsDevice graphicsDevice)
        {
            var output = Path.ChangeExtension(cl.Output, "reflectance.texcube");
            Log.Instance.Write($"Compiling prefiltered reflectance texture cube {Path.GetFileName(output)}");

            Effect effect = null;
            SharedMeshData cube = null;

            await new AsyncUiTask().Run(() => {
                cube = Cube.CreateMeshData(graphicsDevice);

                effect = new EffectBuilder().Start(graphicsDevice)
                .CreateProgram(
                    ShaderBuilder.BuildFromText<VertexShader>(graphicsDevice, ShaderText.VertexShaderCube, null),
                    ShaderBuilder.BuildFromText<PixelShader>(graphicsDevice, ReflectionMapShaderText.PixelShaderPrefiltered, null))
                .Finish();
            }).ConfigureAwait(false);

            effect.Parameters["World"].SetValue(Matrix4x4.Identity);
            effect.Parameters["Projection"].SetValue(CubeFace.CreateProjectionMatrix(0.1f, 1));
            effect.Parameters["AlbedoMap"].SetValue(envMap);

            graphicsDevice.SetVertexBuffer(cube.PositionBuffer);
			graphicsDevice.SetIndexBuffer(cube.IndexBuffer);

            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            int largestMipmapSize = envMap.Size / 4;
            int maxMipLevels = 5;

            var faces = new List<TextureData>[] {
                new List<TextureData>(),
                new List<TextureData>(),
                new List<TextureData>(),
                new List<TextureData>(),
                new List<TextureData>(),
                new List<TextureData>()
            };

            await new AsyncUiTask().Run(() => {
                
                for (int mip = 0; mip < maxMipLevels; mip++)
                {
                    int sz = (int)(largestMipmapSize * MathF.Pow(0.5f, mip));
                    float roughness = (float)mip / (float)(maxMipLevels - 1);
                    effect.Parameters["Roughness"].SetValue(roughness);

                    var rt = new RenderTargetCube(graphicsDevice, sz, false, SurfaceFormat.Rgba, DepthFormat.Depth32F, 0, RenderTargetUsage.PreserveContents);

                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveX);
                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeX);
                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveY);
                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeY);
                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.PositiveZ);
                    CubeFace.WriteMapFace(graphicsDevice, rt, effect, CubeMapFace.NegativeZ);

                    for (int i = 0; i < 6; i++)
                        faces[i].Add(TextureData.FromTextureCube(rt, CubeMapFace.PositiveX + i));
                }
            }).ConfigureAwait(false);

            var texCubeData = TextureCubeWriter.Write(faces[0], faces[1], faces[2], faces[3], faces[4], faces[5]);

            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, texCubeData);
        }
    }
}