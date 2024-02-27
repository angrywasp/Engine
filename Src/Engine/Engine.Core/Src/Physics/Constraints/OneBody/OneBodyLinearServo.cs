using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.OneBodyLinearServo;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OneBodyLinearServo : IConstraint
    {
        private EngineCore engine;
        
        [JsonProperty] public Vector3 LocalOffset;
        [JsonProperty] public Vector3 Target;
        [JsonProperty] public SpringSettings SpringSettings;
        [JsonProperty] public ServoSettings ServoSettings;
        [JsonProperty] public int PhysicsModelIndex;

        private ConstraintHandle handle;

        public void AddToScene(EngineCore engine, List<IConvexShape> bodies)
        {
            this.engine = engine;
            
            handle = engine.Scene.Physics.Simulation.Solver.Add(
                bodies[PhysicsModelIndex].BodyHandle,
                new BepuConstraint
                {
                    LocalOffset = LocalOffset,
                    Target = Target,
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