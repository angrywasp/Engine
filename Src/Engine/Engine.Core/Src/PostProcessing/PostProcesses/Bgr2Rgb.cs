namespace Engine.PostProcessing.PostProcesses
{
    public class Bgr2Rgb : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/Bgr2Rgb.frag.glsl";
    }
}