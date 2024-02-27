namespace Engine.PostProcessing.PostProcesses
{
    public class HorizontalFlip : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/HorizontalFlip.frag.glsl";
    }
}