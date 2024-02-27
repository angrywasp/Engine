using System.Numerics;
using AngryWasp.Logger;
using BepuPhysics;
using BepuPhysics.Collidables;
using Engine.Debug.Shapes.Physics;
using Newtonsoft.Json;
using BepuCylinder = BepuPhysics.Collidables.Cylinder;

namespace Engine.Physics.Collidables
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Cylinder : Shape
    {
        private DebugPhysicsCylinder debugShape;

        [JsonProperty] public float Radius { get; set; } = 0.5f;
        [JsonProperty] public float Length { get; set; } = 1.0f;

        public Cylinder(bool isDynamic)
        {
            this.isDynamic = isDynamic;
        }

        protected override void AddDynamicToScene(EngineCore engine)
        {
            var shape = new BepuCylinder(Radius, Length);
            var speculativeMargin = BodyDescription.GetDefaultSpeculativeMargin(shape);
            BodyInertia inertia = shape.ComputeInertia(Mass);

            shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);

            this.SimulationTransform.Update(Pose.Rotation, Pose.Position);

            var description = BodyDescription.CreateDynamic(Pose.ToRigidPose(),
                Velocity.ToBodyVelocity(), inertia,
                new CollidableDescription(shapeIndex, speculativeMargin),
                BodyDescription.GetDefaultActivity(shape));

            bodyHandle = engine.Scene.Physics.AddDynamicBody(description, Material.ToPhysicsMaterial());
            bodyReference = engine.Scene.Physics.Simulation.Bodies.GetBodyReference(bodyHandle);
            Log.Instance.Write("Physics cylinder shape added to scene");

            debugShape = engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(this);
        }

        protected override void AddStaticToScene(EngineCore engine)
        {
            var shape = new BepuCylinder(Radius, Length);
            shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);

            this.SimulationTransform.Update(Pose.Rotation, Pose.Position);

            var sd = new StaticDescription(Pose.Position, Pose.Rotation, shapeIndex);

            staticHandle = engine.Scene.Physics.AddStaticBody(sd, Material.ToPhysicsMaterial());
            staticReference = engine.Scene.Physics.Simulation.Statics.GetStaticReference(staticHandle);

            debugShape = engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(this);
        }

        public override void AddToBuilder(ref CompoundBuilder builder) =>
            builder.Add(new BepuCylinder(Radius, Length), Pose.ToRigidPose(), Mass);

        public override void RemoveFromScene()
        {
            base.RemoveFromScene();

            engine.Scene.Graphics.DebugRenderer.QueueShapeRemove(debugShape);
            engine.Scene.Physics.Simulation.Shapes.RemoveAndDispose(shapeIndex, engine.Scene.Physics.DefaultBufferPool);
            Log.Instance.Write("Physics cylinder removed from scene");
        }

        public override void Update()
        {
            base.Update();
            debugShape.Update();
        }

        public override string ToString() => "Cylinder";
    }
}
