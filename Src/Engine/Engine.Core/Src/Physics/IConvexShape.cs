using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Newtonsoft.Json;

namespace Engine.Physics
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Pose
    {
        [JsonProperty] public Vector3 Position = Vector3.Zero;
        [JsonProperty] public Quaternion Rotation = Quaternion.Identity;

        public Pose(Vector3 position)
        {
            Position = position;
            Rotation = Quaternion.Identity;
        }

        public Pose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public RigidPose ToRigidPose() => new RigidPose(Position, Rotation);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Velocity
    {
        [JsonProperty] public Vector3 Linear = Vector3.Zero;
        [JsonProperty] public Vector3 Angular = Vector3.Zero;

        public Velocity(Vector3 linear)
        {
            Linear = linear;
            Angular = Vector3.Zero;
        }

        public Velocity(Vector3 linear, Vector3 angular)
        {
            Linear = linear;
            Angular = angular;
        }

        public BodyVelocity ToBodyVelocity() => new BodyVelocity(Linear, Angular);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Material
    {
        [JsonProperty] public float Frequency;
        [JsonProperty] public float DampingRatio;
        [JsonProperty] public float FrictionCoefficient;
        [JsonProperty] public float MaximumRecoveryVelocity;

        public Material(float frequency, float dampingRatio, float frictionCoefficient, float maximumRecoveryVelocity)
        {
            Frequency = frequency;
            DampingRatio = dampingRatio;
            FrictionCoefficient = frictionCoefficient;
            MaximumRecoveryVelocity = maximumRecoveryVelocity;
        }

        public PhysicsMaterial ToPhysicsMaterial() =>
            new PhysicsMaterial
            {
                FrictionCoefficient = FrictionCoefficient,
                MaximumRecoveryVelocity = MaximumRecoveryVelocity,
                SpringSettings = new BepuPhysics.Constraints.SpringSettings(Frequency, DampingRatio)
            };
    }

    public interface IConvexShape
    {
        WorldTransform2 SimulationTransform { get; }
        float Mass { get; set; }
        float InverseMass { get; }
        Pose Pose { get; set; }
        Velocity Velocity { get; set; }
        Material Material { get; set; }
        BodyHandle BodyHandle { get; }
        StaticHandle StaticHandle { get; }
        BodyReference BodyReference { get; }
        StaticReference StaticReference { get; }
        TypedIndex ShapeIndex { get; }
        bool IsDynamic { get; }

        void AddToBuilder(ref CompoundBuilder builder);
        void AddToScene(EngineCore engine);
        void RemoveFromScene();
        void Update();
        void EditorUpdate(Matrix4x4 transform);
    }
}