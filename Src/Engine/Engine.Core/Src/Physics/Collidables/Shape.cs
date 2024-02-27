using System.Numerics;
using AngryWasp.Helpers;
using BepuPhysics;
using BepuPhysics.Collidables;
using Newtonsoft.Json;

namespace Engine.Physics.Collidables
{
    public abstract class Shape : IConvexShape
    {
        private static AsyncLock addLock = new AsyncLock();
        private static AsyncLock removeLock = new AsyncLock();
        private static AsyncLock updateLock = new AsyncLock();

        protected EngineCore engine;
        protected bool isDynamic = false;
        protected BodyHandle bodyHandle;
        protected StaticHandle staticHandle;
        protected BodyReference bodyReference;
        protected StaticReference staticReference;
        protected TypedIndex shapeIndex;

        [JsonProperty] public float Mass { get; set; } = 1.0f;
        [JsonProperty] public Pose Pose { get; set; } = new Pose(Vector3.Zero, Quaternion.Identity);
        [JsonProperty] public Velocity Velocity { get; set; } = new Velocity();
        [JsonProperty] public Material Material { get; set; } = new Material(30, 1, 1, float.MaxValue);
        [JsonProperty] public bool IsDynamic => isDynamic;

        public WorldTransform2 SimulationTransform { get; } = new WorldTransform2();
        public float InverseMass => 1.0f / Mass;
        public BodyHandle BodyHandle => bodyHandle;
        public StaticHandle StaticHandle => staticHandle;
        public BodyReference BodyReference => bodyReference;
        public StaticReference StaticReference => staticReference;
        public TypedIndex ShapeIndex => shapeIndex;

        public virtual void AddToScene(EngineCore engine)
        {
            this.engine = engine;

            var l = addLock.Lock();
            
            if (isDynamic)
                AddDynamicToScene(engine);
            else
                AddStaticToScene(engine);

            l.Dispose();
        }

        protected abstract void AddDynamicToScene(EngineCore engine);

        protected abstract void AddStaticToScene(EngineCore engine);

        public abstract void AddToBuilder(ref BepuPhysics.Collidables.CompoundBuilder builder);

        public virtual void RemoveFromScene()
        {
            var l = removeLock.Lock();

            if (IsDynamic)
                engine.Scene.Physics.RemoveDynamicBody(bodyHandle);
            else
                engine.Scene.Physics.RemoveStaticBody(staticHandle);

            l.Dispose();
        }

        public virtual void Update()
        {
            if (isDynamic)
                this.SimulationTransform.Update(bodyReference.Pose.Orientation, bodyReference.Pose.Position);
            else
                this.SimulationTransform.Update(staticReference.Pose.Orientation, staticReference.Pose.Position);
        }

        public virtual void EditorUpdate(Matrix4x4 transform)
        {
            var l = updateLock.Lock();

            var bodyTransform = Matrix4x4.CreateFromQuaternion(Pose.Rotation) * Matrix4x4.CreateTranslation(Pose.Position);
            var m = bodyTransform * transform;
            Matrix4x4.Decompose(m, out _, out Quaternion rotation, out Vector3 translation);

            var newPose = new Pose(translation, rotation);

            if (IsDynamic)
            {
                bodyReference.GetDescription(out BodyDescription description);
                description.Pose = newPose.ToRigidPose();
                bodyReference.ApplyDescription(description);
            }
            else
            {
                staticReference.GetDescription(out StaticDescription description);
                description.Pose = newPose.ToRigidPose();
                staticReference.ApplyDescription(description);
            }

            l.Dispose();
        }
    }
}