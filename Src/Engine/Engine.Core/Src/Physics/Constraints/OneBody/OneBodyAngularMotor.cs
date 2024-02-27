using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.OneBodyAngularMotor;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OneBodyAngularMotor : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 TargetVelocity;
        [JsonProperty] public MotorSettings MotorSettings;
        [JsonProperty] public int PhysicsModelIndex;

        private ConstraintHandle handle;

        public void AddToScene(EngineCore engine, List<IConvexShape> bodies)
        {
            this.engine = engine;

            handle = engine.Scene.Physics.Simulation.Solver.Add(
                bodies[PhysicsModelIndex].BodyHandle,
                new BepuConstraint
                {
                    TargetVelocity = TargetVelocity,
                    Settings = MotorSettings.ToBepuMotorSettings()
                }
            );
        }

        public void RemoveFromScene()
        {
            engine.Scene.Physics.Simulation.Solver.Remove(handle);
        }
    }
}