using System.Diagnostics;
using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Graphics.Materials
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TerrainMaterial : MaterialBase
    {

        #region Effect parameters

        private EffectParameter worldParam;
        private EffectParameter viewParam;
        private EffectParameter projectionParam;
        private EffectParameter texScaleParam;
        private EffectParameter farClipParam;

        private EffectParameter diffuseMapParam;
        private EffectParameter normalMapParam;

        private EffectParameter diffuseMap0Param;
        private EffectParameter normalMap0Param;
        private EffectParameter diffuseMap1Param;
        private EffectParameter normalMap1Param;
        private EffectParameter diffuseMap2Param;
        private EffectParameter normalMap2Param;
        private EffectParameter diffuseMap3Param;
        private EffectParameter normalMap3Param;

        private EffectParameter blendTextureParam;

        protected EffectParameter gDepthBufferParam;
        protected EffectParameter lColorBufferParam;
        protected EffectParameter lParamsBufferParam;

        protected EffectParameter averageRgbParam;
        protected EffectParameter averageRgb0Param;
        protected EffectParameter averageRgb1Param;
        protected EffectParameter averageRgb2Param;
        protected EffectParameter averageRgb3Param;


        #endregion

        private MaterialClipping clipping;

        private string diffuseMapPath;
        private string normalMapPath;
        private string diffuseMap0Path;
        private string normalMap0Path;
        private string diffuseMap1Path;
        private string normalMap1Path;
        private string diffuseMap2Path;
        private string normalMap2Path;
        private string diffuseMap3Path;
        private string normalMap3Path;
        private string blendTexturePath;

        private Texture2D blendTexture;

        private Vector2 textureScale = Vector2.One;

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

        public string DiffuseMapPath
        {
            get { return diffuseMapPath; }
            set
            {
                diffuseMapPath = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, diffuseMapParam);
            }
        }

        public string NormalMapPath
        {
            get { return normalMapPath; }
            set
            {
                normalMapPath = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMapParam);
            }
        }

        public string DiffuseMap0Path
        {
            get { return diffuseMap0Path; }
            set
            {
                diffuseMap0Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, diffuseMap0Param);
            }
        }

        public string NormalMap0Path
        {
            get { return normalMap0Path; }
            set
            {
                normalMap0Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMap0Param);
            }
        }

        public string DiffuseMap1Path
        {
            get { return diffuseMap1Path; }
            set
            {
                diffuseMap1Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, diffuseMap1Param);
            }
        }

        public string NormalMap1Path
        {
            get { return normalMap1Path; }
            set
            {
                normalMap1Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMap1Param);
            }
        }

        public string DiffuseMap2Path
        {
            get { return diffuseMap2Path; }
            set
            {
                diffuseMap2Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, diffuseMap2Param);
            }
        }

        public string NormalMap2Path
        {
            get { return normalMap2Path; }
            set
            {
                normalMap2Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMap2Param);
            }
        }

        public string DiffuseMap3Path
        {
            get { return diffuseMap3Path; }
            set
            {
                diffuseMap3Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, diffuseMap3Param);
            }
        }

        public string NormalMap3Path
        {
            get { return normalMap3Path; }
            set
            {
                normalMap3Path = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, normalMap3Param);
            }
        }

        public string BlendTexturePath
        {
            get { return blendTexturePath; }
            set
            {
                blendTexturePath = value;
                if (IsLoaded)
                    EffectHelper.LoadTexture(engine.GraphicsDevice, value, blendTextureParam);
            }
        }

        public Texture2D BlendTexture => blendTexture;

        public void ApplyBlendTexture(Texture2D tex) =>
            blendTextureParam.SetValue(tex);

        public MaterialClipping Clipping => clipping;

        public override Effect LoadEffect()
        {
            var basePath = "Engine/Renderer/Shaders/Terrain";
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var renderToGBufferVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/RenderToGBuffer.vert.glsl");
            var reconstructVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Reconstruct.vert.glsl");

            var renderToGBufferPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"{basePath}/RenderToGBuffer.frag.glsl", includeDirs);
            var reconstructPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"{basePath}/Reconstruct.frag.glsl", includeDirs);

            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(renderToGBufferVs, renderToGBufferPs, "RenderToGBuffer")
            .CreateProgram(reconstructVs, reconstructPs, "Reconstruct")
            //Need shadow map
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            worldParam = e.Parameters["World"];
            viewParam = e.Parameters["View"];
            projectionParam = e.Parameters["Projection"];
            texScaleParam = e.Parameters["TexScale"];

            diffuseMapParam = e.Parameters["DiffuseMap"];
            normalMapParam = e.Parameters["NormalMap"];

            diffuseMap0Param = e.Parameters["DiffuseMap0"];
            normalMap0Param = e.Parameters["NormalMap0"];

            diffuseMap1Param = e.Parameters["DiffuseMap1"];
            normalMap1Param = e.Parameters["NormalMap1"];

            diffuseMap2Param = e.Parameters["DiffuseMap2"];
            normalMap2Param = e.Parameters["NormalMap2"];

            diffuseMap3Param = e.Parameters["DiffuseMap3"];
            normalMap3Param = e.Parameters["NormalMap3"];

            blendTextureParam = e.Parameters["BlendTexture"];
            //gDepthBufferParam = e.Parameters["GDepthBuffer"];
            lColorBufferParam = e.Parameters["LColorBuffer"];
            lParamsBufferParam = e.Parameters["LParamsBuffer"];

            //averageRgbParam = e.Parameters["AverageRgb"];
            //averageRgb0Param = e.Parameters["AverageRgb0"];
            //averageRgb1Param = e.Parameters["AverageRgb1"];
            //averageRgb2Param = e.Parameters["AverageRgb2"];
            //averageRgb3Param = e.Parameters["AverageRgb3"];

            farClipParam = e.Parameters["FarClip"];
            clipping = new MaterialClipping(e);
        }

        protected override void LoadResources(EngineCore e)
        {
            
            EffectHelper.LoadTexture(e.GraphicsDevice, diffuseMapPath, diffuseMapParam);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMapPath, normalMapParam);

            EffectHelper.LoadTexture(e.GraphicsDevice, diffuseMap0Path, diffuseMap0Param);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMap0Path, normalMap0Param);

            EffectHelper.LoadTexture(e.GraphicsDevice, diffuseMap1Path, diffuseMap1Param);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMap1Path, normalMap1Param);

            EffectHelper.LoadTexture(e.GraphicsDevice, diffuseMap2Path, diffuseMap2Param);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMap2Path, normalMap2Param);

            EffectHelper.LoadTexture(e.GraphicsDevice, diffuseMap3Path, diffuseMap3Param);
            EffectHelper.LoadTexture(e.GraphicsDevice, normalMap3Path, normalMap3Param);

            //averageRgbParam.SetValue(diffuseMap.AverageRgb);
            //averageRgb0Param.SetValue(diffuseMap0.AverageRgb);
            //averageRgb1Param.SetValue(diffuseMap1.AverageRgb);
            //averageRgb2Param.SetValue(diffuseMap2.AverageRgb);
            //averageRgb3Param.SetValue(diffuseMap3.AverageRgb);

            texScaleParam.SetValue(TextureScale);
            farClipParam.SetValue(engine.Camera.FarClip);
        }

        public override void UpdateTransform(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection)
        {
            worldParam.SetValue(world);
            viewParam.SetValue(view);
            projectionParam.SetValue(projection);
        }

        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer)
        {
            Debugger.Break();
            //lColorBufferParam.SetValue(lBuffer.Color);
            //lParamsBufferParam.SetValue(lBuffer.Parameters);
            //gDepthBufferParam.SetValue(gBuffer.Depth);
        }
    }
}
