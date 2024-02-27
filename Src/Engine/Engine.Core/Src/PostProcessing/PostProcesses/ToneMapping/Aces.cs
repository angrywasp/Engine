namespace Engine.PostProcessing.PostProcesses
{
    public class AcesToneMapping : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ToneMapping/ACES.frag.glsl";
    }
}