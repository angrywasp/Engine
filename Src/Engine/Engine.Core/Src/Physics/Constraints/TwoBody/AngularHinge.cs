using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.AngularHinge;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AngularHinge : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 LocalHingeAxisA;
        [JsonProperty] public Vector3 LocalHingeAxisB;
        [JsonProperty] public SpringSettings SpringSettings;
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
                    LocalHingeAxisA = LocalHingeAxisA,
                    LocalHingeAxisB = LocalHingeAxisB,
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