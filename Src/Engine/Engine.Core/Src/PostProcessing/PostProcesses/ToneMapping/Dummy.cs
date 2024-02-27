namespace Engine.PostProcessing.PostProcesses
{
    public class DummyToneMapping : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ToneMapping/Dummy.frag.glsl";
    }
}