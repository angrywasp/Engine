using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public class AmbientLightEffect
    {
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        private EffectParameter cameraPositionParam;
        private EffectParameter gAlbedoBufferParam;
        private EffectParameter gNormalBufferParam;
        private EffectParameter gPbrBufferParam;
        private EffectParameter gDepthBufferParam;
        private EffectParameter irradianceMapParam;
        private EffectParameter prefilterMapParam;

        public string IrradianceMap { set => EffectHelper.LoadTextureCube(graphicsDevice, value, irradianceMapParam); }

        public string PreFilterMap { set => EffectHelper.LoadTextureCube(graphicsDevice, value, prefilterMapParam); }

        public EffectProgram DefaultProgram => effect.DefaultProgram;

        public AmbientLightEffect(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }
        
        public void Load()
        {
            string[] includeDirs = {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Ambient/RenderToLBuffer.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Lights/Ambient/RenderToLBuffer.frag.glsl", includeDirs);

            effect = new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();

            cameraPositionParam = effect.Parameters["CameraPosition"];
            gAlbedoBufferParam = effect.Parameters["GAlbedoBuffer"];
            gNormalBufferParam = effect.Parameters["GNormalBuffer"];
            gPbrBufferParam = effect.Parameters["GPbrBuffer"];
            gDepthBufferParam = effect.Parameters["GDepthBuffer"];

            irradianceMapParam = effect.Parameters["IrradianceMap"];
            prefilterMapParam = effect.Parameters["PreFilterMap"];

            var brdfLut = ContentLoader.LoadTexture(effect.GraphicsDevice, "Engine/Textures/BrdfLut.texture");
            effect.Parameters["BrdfLutMap"].SetValue(brdfLut);
        }

        public void Update(GBuffer gBuffer, Vector3 cameraPosition)
        {
            gAlbedoBufferParam.SetValue(gBuffer.Albedo);
            gNormalBufferParam.SetValue(gBuffer.Normal);
            gPbrBufferParam.SetValue(gBuffer.PBR);
            gDepthBufferParam.SetValue(gBuffer.Depth);
            cameraPositionParam.SetValue(cameraPosition);
        }
    }
}
