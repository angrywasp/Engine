namespace Engine.PostProcessing.PostProcesses
{
    public class ReinhardToneMapping : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ToneMapping/Reinhard.frag.glsl";
    }
}