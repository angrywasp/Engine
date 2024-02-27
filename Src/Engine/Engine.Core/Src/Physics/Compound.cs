using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;

using BepuCompound = BepuPhysics.Collidables.Compound;

namespace Engine.Physics
{
    public class Compound
    {
        public const float DEFAULT_SPECULATIVE_MARGIN = 0.005f;
        public const float DEFAULT_SLEEP_THRESHOLD = 0.005f;

        private BodyHandle bodyHandle;
        private StaticHandle staticHandle;
        private BodyReference bodyReference;
        private StaticReference staticReference;

        public Material Material { get; set; }= new Material(30, 1, 1, float.MaxValue);
        public List<IConvexShape> Shapes = new List<IConvexShape>();

        private bool isDynamic;
        private EngineCore engine;

        public void AddDynamicToScene(EngineCore engine)
        {
            isDynamic = true;
            this.engine = engine;
            var compound = ToShape(engine);
            var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(compound);

            var description = BodyDescription.CreateDynamic(
                new RigidPose(Vector3.Zero, Quaternion.Identity),
                new BodyVelocity(Vector3.Zero, Vector3.Zero),
                new BodyInertia { InverseMass = 1 },
                new CollidableDescription(shapeIndex, DEFAULT_SPECULATIVE_MARGIN),
                new BodyActivityDescription(DEFAULT_SLEEP_THRESHOLD));

            bodyHandle = engine.Scene.Physics.AddDynamicBody(description, Material.ToPhysicsMaterial());
            bodyReference = engine.Scene.Physics.Simulation.Bodies.GetBodyReference(bodyHandle);
        }

        public void AddStaticToScene(EngineCore engine)
        {
            isDynamic = true;
            this.engine = engine;
            var shape = ToShape(engine);
            var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);

            var description = new StaticDescription(Vector3.Zero, Quaternion.Identity, shapeIndex);

            staticHandle = engine.Scene.Physics.AddStaticBody(description, Material.ToPhysicsMaterial());
            staticReference = engine.Scene.Physics.Simulation.Statics.GetStaticReference(staticHandle);
        }

        public BepuCompound ToShape(EngineCore engine)
        {   
            var p = engine.Scene.Physics;
            CompoundBuilder builder = new CompoundBuilder(p.DefaultBufferPool, p.Simulation.Shapes, Shapes.Count);
            foreach (var shape in Shapes)
                shape.AddToBuilder(ref builder);

            Buffer<CompoundChild> childBuffer;
            BodyInertia inertia;

            builder.BuildDynamicCompound(out childBuffer, out inertia, out _);
            builder.Dispose();

            return new BepuCompound(childBuffer);
        }
    }
}
