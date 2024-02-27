using Engine.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.PostProcessing.PostProcesses
{
    public class FXAA : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/FXAA.frag.glsl";

        public override string[] IncludeDirs => new string[] {
            EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Shaders/Includes")
        };

        public override void UpdateRenderTargets(int w, int h)
        {
            base.UpdateRenderTargets(w, h);
            effect.Parameters["ScreenWidth"].SetValue((float)w);
            effect.Parameters["ScreenHeight"].SetValue((float)h);
        }
    }
}