namespace Engine.PostProcessing.PostProcesses
{
    public class PassThrough : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/PassThrough.frag.glsl";
    }
}