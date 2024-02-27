namespace Engine.PostProcessing.PostProcesses
{
    public class Rgb2Luma : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Rgb2Luma.frag.glsl";
    }
}