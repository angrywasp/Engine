using System.Numerics;
using System.Threading.Tasks;
using Engine.Content;
using Engine.Graphics.Effects;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Graphics.Materials
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WaterMaterial : MaterialBase
    {
        public EffectParameter WorldParam;
        public EffectParameter ViewParam;
        public EffectParameter ProjectionParam;

        public EffectParameter CameraPositionParam;
        public EffectParameter DirectionalLightColor;
        public EffectParameter DirectionalLightDirectionParam;
        public EffectParameter DirectionalLightSpecularPowerParam;
        public EffectParameter DirectionalLightSpecularIntensityParam;

        public EffectParameter ReflectionTextureParam;
        public EffectParameter RefractionTextureParam;
        public EffectParameter WaveMap0Param;
        public EffectParameter WaveMap1Param;

        public EffectParameter WaveMapOffset0Param;
        public EffectParameter WaveMapOffset1Param;

        public EffectParameter WaterColorParam;
        public EffectParameter TexScaleParam;

        public EffectParameter DepthBufferParam;

        private EffectParameter MaximumLightExtinctionParam;
        private EffectParameter WaterHeightParam;
        private EffectParameter ReflectionSourceParam;
        private EffectParameter RefractionSourceParam;

        private string waveMap0;
        private string waveMap1;
        private Vector4 waterColor;
        private float texScale;
        private float maximumLightExtinction;
        private RenderTarget2D reflectionMap;
        private RenderTarget2D refractionMap;
        private RenderTargetBinding[] rBuffer;

        public string WaveMap0
        {
            get => waveMap0;
            set
            {
                waveMap0 = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, WaveMap0Param);
            }
        }

        public string WaveMap1
        {
            get => waveMap1;
            set
            {
                waveMap1 = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, WaveMap1Param);
            }
        }

        public Vector4 WaterColor
        {
            get => waterColor;
            set
            {
                waterColor = value;
                if (IsLoaded)
                    WaterColorParam.SetValue(value);
            }
        }

        public float TexScale{

            get => texScale;
            set
            {
                texScale = value;
                if (IsLoaded)
                    TexScaleParam.SetValue(value);
            }
        }

        public float MaximumLightExtinction
        {
            get => maximumLightExtinction;
            set
            {
                maximumLightExtinction = value;
                if (IsLoaded)
                    MaximumLightExtinctionParam.SetValue(value);
            }
        }

        public RenderTarget2D ReflectionMap => reflectionMap;
        public RenderTarget2D RefractionMap => refractionMap;

        public override Effect LoadEffect()
        {
            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(
                ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Water/Water.vert.glsl"),
                ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Water/Water.frag.glsl"),
                "Water"
            )
            .CreateProgram(
                ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/PostProcessing/PostProcess.vert.glsl"),
                ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Water/Buffer.frag.glsl"),
                "CopyBuffers"
            )
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            WorldParam = e.Parameters["World"];
            ViewParam = e.Parameters["View"];
            ProjectionParam = e.Parameters["Projection"];

            DepthBufferParam = e.Parameters["DepthBuffer"];
            CameraPositionParam = e.Parameters["CameraPosition"];
            DirectionalLightColor = e.Parameters["LightColor"];
            DirectionalLightDirectionParam = e.Parameters["LightDirection"];
            DirectionalLightSpecularPowerParam = e.Parameters["LightSpecularPower"];
            DirectionalLightSpecularIntensityParam = e.Parameters["LightSpecularIntensity"];
            ReflectionTextureParam = e.Parameters["ReflectionTexture"];
            RefractionTextureParam = e.Parameters["RefractionTexture"];
            WaveMap0Param = e.Parameters["WaveMap0"];
            WaveMap1Param = e.Parameters["WaveMap1"];
            WaveMapOffset0Param = e.Parameters["WaveMapOffset0"];
            WaveMapOffset1Param = e.Parameters["WaveMapOffset1"];
            WaterColorParam = e.Parameters["WaterColor"];
            TexScaleParam = e.Parameters["TexScale"];

            MaximumLightExtinctionParam = e.Parameters["MaximumLightExtinction"];
            WaterHeightParam = e.Parameters["WaterHeight"];
            CameraPositionParam = e.Parameters["CameraPosition"];
            ReflectionSourceParam = e.Parameters["ReflectionSource"];
            RefractionSourceParam = e.Parameters["RefractionSource"];
        }

        public async Task CreateRenderTargets(Vector2i renderTargetSize)
        {
            await new AsyncUiTask().Run(() => {
                reflectionMap = new RenderTarget2D(engine.GraphicsDevice, renderTargetSize.X, renderTargetSize.Y, false, SurfaceFormat.Rgba64, DepthFormat.None);
                refractionMap = new RenderTarget2D(engine.GraphicsDevice, renderTargetSize.X, renderTargetSize.Y, false, SurfaceFormat.Rgba64, DepthFormat.None);

                rBuffer = new RenderTargetBinding[]
                {
                    reflectionMap,
                    refractionMap
                };
            }).ConfigureAwait(false);
        }

        public override void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            WorldParam.SetValue(world);
            ViewParam.SetValue(view);
            ProjectionParam.SetValue(projection);
            WaterHeightParam.SetValue(world.Translation.Y);
        }

        public void CopyBuffers(Texture2D reflectionSource, Texture2D refractionSource)
        {
            ReflectionSourceParam.SetValue(reflectionSource);
            RefractionSourceParam.SetValue(refractionSource);
            engine.GraphicsDevice.SetRenderTargets(rBuffer);
            engine.GraphicsDevice.Clear(Vector4.Zero);
            effect.Programs[1].Apply();
            engine.Scene.Graphics.Quad.RenderQuad();
            engine.GraphicsDevice.SetRenderTarget(engine.Scene.Graphics.OutputTexture);
        }

        public void ApplyBuffers()
        {
            ReflectionTextureParam.SetValue(reflectionMap);
            RefractionTextureParam.SetValue(refractionMap);
        }

        protected override void LoadResources(EngineCore engine)
        {
            EffectHelper.LoadTexture(engine.GraphicsDevice, waveMap0, WaveMap0Param);
            EffectHelper.LoadTexture(engine.GraphicsDevice, waveMap1, WaveMap1Param);
            WaterColorParam.SetValue(waterColor);
            TexScaleParam.SetValue(texScale);
            MaximumLightExtinctionParam.SetValue(maximumLightExtinction);

            //todo: need to pull these values from a skybox if opresent
            DirectionalLightColor.SetValue(Color.White.ToVector4());
            DirectionalLightDirectionParam.SetValue(new Vector3(1, -1, 1));
            DirectionalLightSpecularPowerParam.SetValue(1f);
            DirectionalLightSpecularIntensityParam.SetValue(1f);
        }
    }
}
