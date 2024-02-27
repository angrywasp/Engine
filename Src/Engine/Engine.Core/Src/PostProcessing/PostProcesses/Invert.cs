namespace Engine.PostProcessing.PostProcesses
{
    public class Invert : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Invert.frag.glsl";
    }
}