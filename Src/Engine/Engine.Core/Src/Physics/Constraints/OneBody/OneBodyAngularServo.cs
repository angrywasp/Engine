using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.OneBodyAngularServo;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OneBodyAngularServo : IConstraint
    {
        private EngineCore engine;
        
        [JsonProperty] public Quaternion TargetOrientation;
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
                    TargetOrientation = TargetOrientation,
                    ServoSettings = ServoSettings.ToBepuServoSettings(),
                    SpringSettings = SpringSettings.ToBepuSpringSettings()
                }
            );
        }

        public void RemoveFromScene()
        {
            engine.Scene.Physics.Simulation.Solver.Remove(handle);
        }
    }
}