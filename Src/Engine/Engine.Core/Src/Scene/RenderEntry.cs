using Engine.Content;
using Engine.World.Components.Lights;

namespace Engine.Scene
{
    public struct DirectionalLightEntry
    {
        public DirectionalLightComponent Light;
        public DirectionalShadowMapEntry ShadowMap;
        public float Priority;
    }

    public struct SpotLightEntry
    {
        public SpotLightComponent Light;
        public SpotShadowMapEntry ShadowMap;
        public float Priority;
    }

    struct ParticleEntry
    {
        public ParticleSystem ParticleSystem;
        public float Priority;
    }
}
