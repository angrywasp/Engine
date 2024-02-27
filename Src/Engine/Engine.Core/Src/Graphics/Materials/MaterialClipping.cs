using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Materials
{
    public class MaterialClipping
    {
        private EffectParameter clipHeightParam;
        private EffectParameter doClipParam;

        public MaterialClipping(Effect e)
        {
            clipHeightParam = e.Parameters["ClipHeight"];
            doClipParam = e.Parameters["DoClip"];
            doClipParam.SetValue(false);
            clipHeightParam.SetValue(0.0f);
        }

        public void Enable(float clipHeight)
        {
            doClipParam.SetValue(true);
            clipHeightParam.SetValue(clipHeight);
        }

        public void Disable()
        {
            doClipParam.SetValue(false);
        }
    }
}
