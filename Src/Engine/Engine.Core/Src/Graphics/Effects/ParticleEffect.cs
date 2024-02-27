using System.Numerics;
using Engine.Content;
using Engine.Graphics.Effects.Builder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Graphics.Effects
{
    public class ParticleEffect : WorldEffect
    {
        private EffectParameter viewportScaleParam;
        private EffectParameter timeParam;
        private EffectParameter durationParam;
        private EffectParameter durationRandomnessParam;
        private EffectParameter gravityParam;
        private EffectParameter endVelocityParam;
        private EffectParameter minColorParam;
        private EffectParameter maxColorParam;
        private EffectParameter rotateSpeedParam;
        private EffectParameter startSizeParam;
        private EffectParameter endSizeParam;
        protected EffectParameter textureParam;

        public Vector2 ViewportScale { set => viewportScaleParam.SetValue(value); }
        public float Time { set => timeParam.SetValue(value); }
        public float Duration { set => durationParam.SetValue(value); }
        public float DurationRandomness { set => durationRandomnessParam.SetValue(value); }
        public Vector3 Gravity { set => gravityParam.SetValue(value); }
        public float Endvelocity { set => endVelocityParam.SetValue(value); }
        public Color MinColor { set => minColorParam.SetValue(value.ToVector4()); }
        public Color MaxColor { set => maxColorParam.SetValue(value.ToVector4()); }
        public Vector2 RotateSpeed { set => rotateSpeedParam.SetValue(value); }
        public Vector2 StartSize { set => startSizeParam.SetValue(value); }
        public Vector2 EndSize { set => endSizeParam.SetValue(value); }
        public string TexturePath { set => EffectHelper.LoadTexture(graphicsDevice, value, textureParam); }


        protected override Effect LoadEffect()
        {
            var vs = ContentLoader.LoadShader<VertexShader>(graphicsDevice, "Engine/Renderer/Shaders/Particle/Particle.vert.glsl");
            var ps = ContentLoader.LoadShader<PixelShader>(graphicsDevice, "Engine/Renderer/Shaders/Particle/Particle.frag.glsl");

            return new EffectBuilder().Start(graphicsDevice)
            .CreateProgram(vs, ps)
            .Finish();
        }

        protected override void ExtractParameters(Effect effect)
        {
            base.ExtractParameters(effect);

            viewportScaleParam = effect.Parameters["ViewportScale"];
            timeParam = effect.Parameters["CurrentTime"];
            durationParam = effect.Parameters["Duration"];
            durationRandomnessParam = effect.Parameters["DurationRandomness"];
            gravityParam = effect.Parameters["Gravity"];
            endVelocityParam = effect.Parameters["EndVelocity"];
            minColorParam = effect.Parameters["MinColor"];
            maxColorParam = effect.Parameters["MaxColor"];
            rotateSpeedParam = effect.Parameters["RotateSpeed"];
            startSizeParam = effect.Parameters["StartSize"];
            endSizeParam = effect.Parameters["EndSize"];
            textureParam = effect.Parameters["Texture"];
        }

        public void Apply() => effect.DefaultProgram.Apply();
    }
}