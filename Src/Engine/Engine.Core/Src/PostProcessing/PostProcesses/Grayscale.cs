namespace Engine.PostProcessing.PostProcesses
{
    public class Grayscale : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Grayscale.frag.glsl";
    }
}