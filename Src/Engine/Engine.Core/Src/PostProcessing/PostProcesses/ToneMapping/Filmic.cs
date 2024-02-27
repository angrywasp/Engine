namespace Engine.PostProcessing.PostProcesses
{
    public class FilmicToneMapping : PostProcessBase
    {
        public override string EffectFile => "Engine/Renderer/Shaders/PostProcessing/ToneMapping/Filmic.frag.glsl";
    }
}