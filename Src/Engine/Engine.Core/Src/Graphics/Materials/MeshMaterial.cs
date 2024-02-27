using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Content;
using Engine.Graphics.Effects;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Materials
{
    public class MeshMaterial : MaterialBase
    {
        #region Effect parameters

        protected EffectParameter worldParam;
        protected EffectParameter viewParam;
        protected EffectParameter projectionParam;

        protected EffectParameter texScaleParam;
        protected EffectParameter farClipParam;

        protected EffectParameter albedoMapParam;
        protected EffectParameter normalMapParam;
        protected EffectParameter pbrMapParam;
        protected EffectParameter emissiveMapParam;

        protected EffectParameter lRadianceBufferParam;
        protected EffectParameter lAmbientBufferParam;

        #endregion

        protected MaterialClipping clipping;

        protected string albedoMap = "Engine/Textures/Default_albedo.texture";
        protected string normalMap = "Engine/Textures/Default_normal.texture";
        protected string pbrMap = "Engine/Textures/DefaultRoughness255_pbr.texture";
        protected string emissiveMap = "Engine/Textures/Default_emissive.texture";
        protected Vector2 textureScale = Vector2.One;
        protected RasterizerState rasterizerState = RasterizerState.CullCounterClockwise;
        protected RasterizerState shadowRasterizerState = RasterizerState.CullClockwise;
        protected bool doubleSided = false;

        public bool DoubleSided
        {
            get { return doubleSided; }
            set
            {
                doubleSided = value;
                rasterizerState = value ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise;
                shadowRasterizerState = value ? RasterizerState.CullNone : RasterizerState.CullClockwise;
            }
        }

        public RasterizerState RasterizerState => rasterizerState;
        public RasterizerState ShadowRasterizerState => shadowRasterizerState;

        public Vector2 TextureScale
        {
            get { return textureScale; }
            set
            {
                textureScale = value;
                if (IsLoaded)
                    texScaleParam.SetValue(value);
            }
        }

        public string AlbedoMap
        {
            get { return albedoMap; }
            set
            {
                albedoMap = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, albedoMapParam);
            }
        }

        public string NormalMap
        {
            get { return normalMap; }
            set
            {
                normalMap = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMapParam);
            }
        }

        public string PbrMap
        {
            get { return pbrMap; }
            set
            {
                pbrMap = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, pbrMapParam);
            }
        }

        public string EmissiveMap
        {
            get { return emissiveMap; }
            set
            {
                emissiveMap = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, emissiveMapParam);
            }
        }

        public MaterialClipping Clipping => clipping;

        public override Effect LoadEffect()
        {
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var renderToGBufferVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/RenderToGBuffer.vert.glsl");
            var reconstructVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Reconstruct.vert.glsl");
            var shadowMapVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/ShadowMap.vert.glsl");
            var renderToGBufferInstancedVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/RenderToGBuffer.instanced.vert.glsl");
            var reconstructInstancedVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Reconstruct.instanced.vert.glsl");
            var shadowMapInstancedVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/ShadowMap.instanced.vert.glsl");

            var renderToGBufferPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/RenderToGBuffer.frag.glsl", includeDirs);
            var reconstructPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/Reconstruct.frag.glsl", includeDirs);
            var shadowMapPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/ShadowMap.frag.glsl", includeDirs);

            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(renderToGBufferVs, renderToGBufferPs, "RenderToGBuffer")
            .CreateProgram(renderToGBufferInstancedVs, renderToGBufferPs, "RenderToGBufferInstanced")
            .CreateProgram(reconstructVs, reconstructPs, "Reconstruct")
            .CreateProgram(reconstructInstancedVs, reconstructPs, "ReconstructInstanced")
            .CreateProgram(shadowMapVs, shadowMapPs, "ShadowMap")
            .CreateProgram(shadowMapInstancedVs, shadowMapPs, "ShadowMapInstanced")
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            worldParam = e.Parameters["World"];
            viewParam = e.Parameters["View"];
            projectionParam = e.Parameters["Projection"];

            texScaleParam = e.Parameters["TexScale"];
            farClipParam = e.Parameters["FarClip"];

            albedoMapParam = e.Parameters["AlbedoMap"];
            normalMapParam = e.Parameters["NormalMap"];
            pbrMapParam = e.Parameters["PbrMap"];
            emissiveMapParam = e.Parameters["EmissiveMap"];

            lRadianceBufferParam = e.Parameters["LRadianceBuffer"];
            lAmbientBufferParam = e.Parameters["LAmbientBuffer"];
            
            clipping = new MaterialClipping(e);
        }

        protected override void LoadResources(EngineCore e)
        {
            EffectHelper.LoadTexture(e.GraphicsDevice, albedoMap, albedoMapParam);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMap, normalMapParam);
            EffectHelper.LoadTexture(e.GraphicsDevice, pbrMap, pbrMapParam);
            EffectHelper.LoadTexture(e.GraphicsDevice, emissiveMap, emissiveMapParam);

            texScaleParam.SetValue(TextureScale);
            farClipParam?.SetValue(engine.Camera.FarClip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            worldParam.SetValue(world);
            viewParam.SetValue(view);
            projectionParam.SetValue(projection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBuffers(GBuffer gBuffer, LBuffer lBuffer)
        {
            lRadianceBufferParam?.SetValue(lBuffer.Radiance);
            lAmbientBufferParam?.SetValue(lBuffer.Ambient);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetBones(Matrix4x4[] boneTransforms) { }
    }
}
