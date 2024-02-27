using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects.Lights
{
    public abstract class MeshLightEffect : LightEffect
    {
        public EffectParameter WorldParam;
        public EffectParameter ViewParam;
        public EffectParameter ProjectionParam;
        public EffectParameter LightPositionParam;
        public EffectParameter InvLightRadiusSqrParam;

        protected override abstract Effect LoadEffect();

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);

            WorldParam = effect.Parameters["World"];
            ViewParam = effect.Parameters["View"];
            ProjectionParam = effect.Parameters["Projection"];
            LightPositionParam = effect.Parameters["LightPosition"];
            InvLightRadiusSqrParam = effect.Parameters["InvLightRadiusSqr"];
        }
    }
}