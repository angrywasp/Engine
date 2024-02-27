using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine;
using Engine.AssetTransport;
using Engine.Bitmap.Data;
using Engine.Game;
using Engine.Graphics.Effects.Builder;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextureGenerator
{
    public class Processor
    {
        public async Task Process(ConsoleGame cg, CommandLine cl)
        {
            Log.Instance.Write($"Generating engine textures");

            Directory.CreateDirectory(cl.Output);
            Directory.CreateDirectory(Path.Combine(cl.Output, "Textures/Colors"));
            Directory.CreateDirectory(Path.Combine(cl.Output, "Textures"));
            Directory.CreateDirectory(Path.Combine(cl.Output, "Textures/Cubes"));

            //if (!File.Exists(Path.Combine(cl.Output, "BrdfLut.texture")))
            {
                var graphicsDevice = cg.GraphicsDevice;
                QuadRenderer quad = null;
                Effect effect = null;

                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                await new AsyncUiTask().Run(() => {
                    effect = new EffectBuilder().Start(graphicsDevice)
                    .CreateProgram(
                        ShaderBuilder.BuildFromText<VertexShader>(graphicsDevice, ShaderText.VertexShader, null),
                        ShaderBuilder.BuildFromText<PixelShader>(graphicsDevice, ShaderText.PixelShaderBRDFIntegration, null))
                    .Finish();
                }).ConfigureAwait(false);

                await new AsyncUiTask().Run(() => {
                    var format = SurfaceFormat.HalfVector2;
                    var sz = new Vector2i(1024, 1024);

                    quad = new QuadRenderer(graphicsDevice, sz);
                    var rt = new RenderTarget2D(graphicsDevice, sz.X, sz.Y, true, format, DepthFormat.Depth32F, 0, RenderTargetUsage.PreserveContents);
                    
                    graphicsDevice.SetRenderTarget(rt);
                    graphicsDevice.Clear(Color.Black.ToVector4());

                    effect.DefaultProgram.Apply();
                    quad.RenderQuad();

                    var data = new byte[1024 * 1024 * format.GetSize()];
                    rt.GetData(data);

                    File.WriteAllBytes(Path.Combine(cl.Output, "Textures/BrdfLut.texture"), Texture2DWriter.Write(sz.X, sz.Y, format, data));
                });
            }

            //Color textures
            CreateColorTexture(cl, Color.Black, nameof(Color.Black));
            CreateColorTexture(cl, Color.Blue, nameof(Color.Blue));
            CreateColorTexture(cl, Color.Cyan, nameof(Color.Cyan));
            CreateColorTexture(cl, Color.DeepSkyBlue, nameof(Color.DeepSkyBlue));
            CreateColorTexture(cl, Color.Gray, nameof(Color.Gray));
            CreateColorTexture(cl, Color.Green, nameof(Color.Green));
            CreateColorTexture(cl, Color.GreenYellow, nameof(Color.GreenYellow));
            CreateColorTexture(cl, Color.Gold, nameof(Color.Gold));
            CreateColorTexture(cl, Color.Magenta, nameof(Color.Magenta));
            CreateColorTexture(cl, Color.Orange, nameof(Color.Orange));
            CreateColorTexture(cl, Color.Pink, nameof(Color.Pink));
            CreateColorTexture(cl, Color.Purple, nameof(Color.Purple));
            CreateColorTexture(cl, Color.Red, nameof(Color.Red));
            CreateColorTexture(cl, Color.White, nameof(Color.White));
            CreateColorTexture(cl, Color.Yellow, nameof(Color.Yellow));

            //Default material textures
            CreateMaterialTexture(Texture_Type.Albedo, cl, 128, 128, 128, 255, "Default");
            CreateMaterialTexture(Texture_Type.Normal, cl, 128, 128, 255, 255, "Default");
            CreateMaterialTexture(Texture_Type.Emissive, cl, 0, 0, 0, 255, "Default");

            byte[] pbrRoghnessValues = { 0, 1, 7, 15, 31, 63, 95, 127, 159, 191, 223, 255 };

            foreach (var r in pbrRoghnessValues)
            {
                CreateMaterialTexture(Texture_Type.Pbr, cl, 0, 255, r, 255, $"DefaultRoughness{r}");
                CreateMaterialTexture(Texture_Type.Pbr, cl, 255, 255, r, 255, $"DefaultMetallicRoughness{r}");
            }

            var p = Path.Combine(cl.Output, "Textures/Cubes/Default.texcube");
            if (!File.Exists(p))
            {
                var s = new List<TextureData2D> { TextureData2D.Default(Color.Gray, 1, 1) };
                File.WriteAllBytes(p, TextureCubeWriter.Write(s, s, s, s, s, s));
            }

            p = Path.Combine(cl.Output, "Textures/Cubes/Black.texcube");
            if (!File.Exists(p))
            {
                var s = new List<TextureData2D> { TextureData2D.Default(Color.Black, 1, 1) };
                File.WriteAllBytes(p, TextureCubeWriter.Write(s, s, s, s, s, s));
            }

            p = Path.Combine(cl.Output, "Textures/Cubes/White.texcube");
            if (!File.Exists(p))
            {
                var s = new List<TextureData2D> { TextureData2D.Default(Color.Black, 1, 1) };
                File.WriteAllBytes(p, TextureCubeWriter.Write(s, s, s, s, s, s));
            }

            await cg.ExitGameAsync().ConfigureAwait(false);
        }

        private void CreateColorTexture(CommandLine cl, Color c, string name)
        {
            var p = Path.Combine(cl.Output, $"Textures/Colors/{name}_albedo.texture");
            if (File.Exists(p))
                return;

            File.WriteAllBytes(p, Texture2DWriter.Write(1, 1, SurfaceFormat.Rgba, new byte[] { c.R, c.G, c.B, c.A }));
        }

        private void CreateMaterialTexture(Texture_Type textureType, CommandLine cl, byte r, byte g, byte b, byte a, string name)
        {
            var p = Path.Combine(cl.Output, $"Textures/{name}_{textureType.ToString().ToLower()}.texture");
            if (File.Exists(p))
                return;

            File.WriteAllBytes(p, Texture2DWriter.Write(1, 1, SurfaceFormat.Rgba, new byte[] { r, g, b, a }));
        }
    }
}