using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.TwistServo;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TwistServo : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Quaternion LocalBasisA;
        [JsonProperty] public Quaternion LocalBasisB;
        [JsonProperty] public float TargetAngle;
        [JsonProperty] public SpringSettings SpringSettings;
        [JsonProperty] public ServoSettings ServoSettings;
        [JsonProperty] public int PhysicsModelIndexA;
        [JsonProperty] public int PhysicsModelIndexB;

        private ConstraintHandle handle;

        public void AddToScene(EngineCore engine, List<IConvexShape> bodies)
        {
            this.engine = engine;
            
            handle = engine.Scene.Physics.Simulation.Solver.Add(
                bodies[PhysicsModelIndexA].BodyHandle,
                bodies[PhysicsModelIndexB].BodyHandle,
                new BepuConstraint
                {
                    LocalBasisA = LocalBasisA,
                    LocalBasisB = LocalBasisB,
                    TargetAngle = TargetAngle,
                    SpringSettings = SpringSettings.ToBepuSpringSettings(),
                    ServoSettings = ServoSettings.ToBepuServoSettings()
                }
            );
        }

        public void RemoveFromScene()
        {
            engine.Scene.Physics.Simulation.Solver.Remove(handle);
        }
    }
}