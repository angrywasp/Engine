using System.Numerics;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Graphics.Materials
{
    /*[JsonObject(MemberSerialization.OptIn)]
    public class TessellatedMeshMaterial : MaterialBase
    {

        #region Effect parameters

        private EffectParameter worldParam;
        private EffectParameter viewParam;
        private EffectParameter projectionParam;
        private EffectParameter texScaleParam;
        private EffectParameter farClipParam;
        private EffectParameter bonesParam;

        private EffectParameter diffuseMapParam;
        private EffectParameter normalMapParam;
        private EffectParameter specularMapParam;
        private EffectParameter emissiveMapParam;
        private EffectParameter pbrMapParam;

        protected EffectParameter lColorBufferParam;
        protected EffectParameter lParamsBufferParam;

        #endregion

        private MaterialClipping clipping;

        private string diffuseMap;
        private string normalMap;
        private string specularMap;
        private string emissiveMap;
        private string pbrMap;
        private Vector2 textureScale = Vector2.One;
        private RasterizerState rasterizerState = RasterizerState.CullCounterClockwise;
        private RasterizerState shadowRasterizerState = RasterizerState.CullClockwise;
        private bool doubleSided;

        [JsonProperty]
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

        [JsonProperty]
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

        [JsonProperty]
        public string AlbedoMap
        {
            get { return albedoMap; }
            set
            {
                albedoMap = value;
                if (IsLoaded)
                    LoadTexture(value, albedoMapParam);
            }
        }

        [JsonProperty]
        public string NormalMap
        {
            get { return normalMap; }
            set
            {
                normalMap = value;
                if (IsLoaded)
                    LoadTexture(value, normalMapParam);
            }
        }

        [JsonProperty]
        public string PbrMap
        {
            get { return pbrMap; }
            set
            {
                pbrMap = value;
                if (IsLoaded)
                    LoadTexture(value, pbrMapParam);
            }
        }

        [JsonProperty]
        public string EmissiveMap
        {
            get { return emissiveMap; }
            set
            {
                emissiveMap = value;
                if (IsLoaded)
                    LoadTexture(value, emissiveMapParam);
            }
        }

        [JsonProperty]
        public string PbrMap
        {
            get { return pbrMap; }
            set
            {
                pbrMap = value;
                if (IsLoaded)
                    LoadTexture(value, pbrMapParam);
            }
        }

        public MaterialClipping Clipping => clipping;

        public static MeshMaterial Create(string albedoMap, string normalMap, string pbrMap, string emissiveMap, bool doubleSided = false)
        {
            MeshMaterial m = new MeshMaterial();
            m.AlbedoMap = albedoMap;
            m.NormalMap = normalMap;
            m.PbrMap = pbrMap;
            m.EmissiveMap = emissiveMap;
            m.DoubleSided = doubleSided;

            return m;
        }

        public void SetBones(Matrix4x4[] boneTransforms)
        {
            bonesParam.SetValue(boneTransforms);
        }

        public override Effect LoadEffect()
        {
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var renderToGBufferVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Tesselated/RenderToGBuffer.vert.glsl");
            var renderToGBufferTc = ContentLoader.LoadShader<TessellationControlShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Tesselated/RenderToGBuffer.tc.glsl");
            var renderToGBufferTe = ContentLoader.LoadShader<TessellationEvaluationShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Tesselated/RenderToGBuffer.te.glsl");
            var renderToGBufferPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Tesselated/RenderToGBuffer.frag.glsl", includeDirs);

            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(renderToGBufferVs, renderToGBufferTc, renderToGBufferTe, renderToGBufferPs, "RenderToGBuffer")
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            worldParam = effect.Parameters["World"];
            viewParam = effect.Parameters["View"];
            projectionParam = effect.Parameters["Projection"];
            texScaleParam = effect.Parameters["TexScale"];

            diffuseMapParam = effect.Parameters["DiffuseMap"];
            normalMapParam = effect.Parameters["NormalMap"];
            //specularMapParam = effect.Parameters["SpecularMap"];
            //emissiveMapParam = effect.Parameters["EmissiveMap"];
            pbrMapParam = effect.Parameters["PbrMap"];

            //lColorBufferParam = effect.Parameters["LColorBuffer"];
            //lParamsBufferParam = effect.Parameters["LParamsBuffer"];

            //bonesParam = effect.Parameters["Bones"];

            farClipParam = effect.Parameters["FarClip"];
            clipping = new MaterialClipping(effect);
        }

        protected override void LoadResources(EngineCore effect)
        {
            LoadTexture(diffuseMap, diffuseMapParam);
            LoadTexture(normalMap, normalMapParam);
            LoadTexture(specularMap, specularMapParam);
            LoadTexture(emissiveMap, emissiveMapParam);
            LoadTexture(pbrMap, pbrMapParam);

            texScaleParam.SetValue(TextureScale);
            farClipParam.SetValue(engine.Camera.FarClip);
        }

        public override void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            worldParam.SetValue(world);
            viewParam.SetValue(view);
            projectionParam.SetValue(projection);
        }

        public void SetLightBuffer(LBuffer lBuffer)
        {
            lColorBufferParam.SetValue(lBuffer.Color);
            lParamsBufferParam.SetValue(lBuffer.Parameters);
        }

        public override void Apply(int pass)
        {
            engine.GraphicsDevice.SetPatchVertexCount(3);
            base.Apply(pass);
        }
    }*/
}
