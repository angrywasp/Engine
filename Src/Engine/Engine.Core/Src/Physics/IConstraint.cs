using System.Collections.Generic;
using Newtonsoft.Json;
using BepuMotorSettings = BepuPhysics.Constraints.MotorSettings;
using BepuServoSettings = BepuPhysics.Constraints.ServoSettings;
using BepuSpringSettings = BepuPhysics.Constraints.SpringSettings;

namespace Engine.Physics
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct MotorSettings
    {
        [JsonProperty] public float MaximumForce;
        [JsonProperty] public float Softness;

        public MotorSettings(float maximumForce, float softness)
        {
            MaximumForce = maximumForce;
            Softness = softness;
        }

        public BepuMotorSettings ToBepuMotorSettings() => new BepuMotorSettings(MaximumForce, Softness);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct ServoSettings
    {
        [JsonProperty] public float MaximumSpeed;
        [JsonProperty] public float BaseSpeed;
        [JsonProperty] public float MaximumForce;

        public ServoSettings(float maximumSpeed, float baseSpeed, float maximumForce)
        {
            MaximumSpeed = maximumSpeed;
            BaseSpeed = baseSpeed;
            MaximumForce = maximumForce;
        }

        public BepuServoSettings ToBepuServoSettings() => new BepuServoSettings(MaximumSpeed, BaseSpeed, MaximumForce);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct SpringSettings
    {
        [JsonProperty] public float Frequency;
        [JsonProperty] public float DampingRatio;

        public SpringSettings(float frequency, float dampingRatio)
        {
            Frequency = frequency;
            DampingRatio = dampingRatio;
        }

        public BepuSpringSettings ToBepuSpringSettings() => new BepuSpringSettings(Frequency, DampingRatio);
    }

    public interface IConstraint
    {
        void AddToScene(EngineCore engine, List<IConvexShape> bodies);
        void RemoveFromScene();
    }
}