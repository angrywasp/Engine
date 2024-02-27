using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.BallSocketServo;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BallSocketServo : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 LocalOffsetA;
        [JsonProperty] public Vector3 LocalOffsetB;
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
                    LocalOffsetA = LocalOffsetA,
                    LocalOffsetB = LocalOffsetB,
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