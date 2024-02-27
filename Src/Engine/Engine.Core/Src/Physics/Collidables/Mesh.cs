using System;
using System.Numerics;
using AngryWasp.Logger;
using BepuPhysics;
using BepuPhysics.Collidables;
using Engine.Content;
using Engine.Debug.Shapes.Physics;
using Newtonsoft.Json;
using BepuMesh = BepuPhysics.Collidables.Mesh;

namespace Engine.Physics.Collidables
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Mesh : Shape
    {
        private DebugPhysicsMesh debugShape;

        [JsonProperty] public string Path { get; set; } = string.Empty;

        private BepuPhysics.Collidables.Mesh shape;

        public override void AddToScene(EngineCore engine)
        {
            this.engine = engine;
            AddStaticToScene(engine);
        }

        protected override void AddDynamicToScene(EngineCore engine) => throw new InvalidOperationException("Physics meshes do not support being dynamic");

        protected override void AddStaticToScene(EngineCore engine)
        {
            this.engine = engine;
            shape = ContentLoader.LoadCollisionMesh(engine, Path);
            shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);

            this.SimulationTransform.Update(Pose.Rotation, Pose.Position);

            var sd = new StaticDescription(Pose.Position, Pose.Rotation, shapeIndex);

            staticHandle = engine.Scene.Physics.AddStaticBody(sd, Material.ToPhysicsMaterial());
            staticReference = engine.Scene.Physics.Simulation.Statics.GetStaticReference(staticHandle);

            debugShape = engine.Scene.Graphics.DebugRenderer.QueueShapeAdd(this, shape.Triangles);
        }

        public override void AddToBuilder(ref CompoundBuilder builder) =>
            throw new InvalidOperationException("Collision meshes cannot be added in compound bodies");

        public override void RemoveFromScene()
        {
            engine.Scene.Physics.RemoveStaticBody(staticHandle);
        }

        public override void Update()
        {
            this.SimulationTransform.Update(staticReference.Pose.Orientation, staticReference.Pose.Position);
            debugShape.Update();
        }

        public override void EditorUpdate(Matrix4x4 transform)
        {
            var bodyTransform = Matrix4x4.CreateFromQuaternion(Pose.Rotation) * Matrix4x4.CreateTranslation(Pose.Position);
            var m = bodyTransform * transform;
            Matrix4x4.Decompose(m, out _, out Quaternion rotation, out Vector3 translation);

            var newPose = new Pose(translation, rotation);

            staticReference.GetDescription(out StaticDescription description);
            description.Pose = newPose.ToRigidPose();
            staticReference.ApplyDescription(description);
        }

        public override string ToString() => "Mesh";
    }
}
