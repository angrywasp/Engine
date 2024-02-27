using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.AngularMotor;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AngularMotor : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 TargetVelocityLocalA;
        [JsonProperty] public MotorSettings MotorSettings;
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
                    TargetVelocityLocalA = TargetVelocityLocalA,
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