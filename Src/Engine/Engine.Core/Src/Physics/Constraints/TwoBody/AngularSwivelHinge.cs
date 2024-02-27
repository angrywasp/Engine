using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using Newtonsoft.Json;
using BepuConstraint = BepuPhysics.Constraints.AngularSwivelHinge;

namespace Engine.Physics.Constraints
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AngularSwivelHinge : IConstraint
    {
        private EngineCore engine;

        [JsonProperty] public Vector3 LocalSwivelAxisA;
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
                    LocalSwivelAxisA = LocalSwivelAxisA,
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