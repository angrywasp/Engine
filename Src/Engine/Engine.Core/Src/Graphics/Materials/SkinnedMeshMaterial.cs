using System.Numerics;
using System.Runtime.CompilerServices;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Materials
{
    public class SkinnedMeshMaterial : MeshMaterial
    {
        protected EffectParameter bonesParam;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetBones(Matrix4x4[] boneTransforms) => bonesParam.SetValue(boneTransforms);

        public override Effect LoadEffect()
        {
            var includeDirs = new string[] {
                EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
            };

            var renderToGBufferVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/RenderToGBuffer.skinned.vert.glsl", includeDirs);
            var reconstructVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, "Engine/Renderer/Shaders/Reconstruct.skinned.vert.glsl", includeDirs);
            var shadowMapVs = ContentLoader.LoadShader<VertexShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/ShadowMap.skinned.vert.glsl", includeDirs);

            var renderToGBufferPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/RenderToGBuffer.frag.glsl", includeDirs);
            var reconstructPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/Reconstruct.frag.glsl", includeDirs);
            var shadowMapPs = ContentLoader.LoadShader<PixelShader>(engine.GraphicsDevice, $"Engine/Renderer/Shaders/Mesh/ShadowMap.frag.glsl", includeDirs);

            return new EffectBuilder().Start(engine.GraphicsDevice)
            .CreateProgram(renderToGBufferVs, renderToGBufferPs, "RenderToGBuffer")
            .CreateProgram(reconstructVs, reconstructPs, "Reconstruct")
            .CreateProgram(shadowMapVs, shadowMapPs, "ShadowMap")
            .Finish();
        }

        public override void ExtractParameters(Effect e)
        {
            base.ExtractParameters(e);
            bonesParam = e.Parameters["Bones"];
        }
    }
}
