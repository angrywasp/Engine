using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.LinearAxisMotor;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LinearAxisMotor : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 LocalOffsetA;
        [JsonProperty] public Vector3 LocalOffsetB;
        [JsonProperty] public Vector3 LocalAxis;
        [JsonProperty] public Vector3 LocalPlaneNormal;
        [JsonProperty] public float TargetVelocity;
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
                    LocalOffsetA = LocalOffsetA,
                    LocalOffsetB = LocalOffsetB,
                    LocalAxis = LocalAxis,
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