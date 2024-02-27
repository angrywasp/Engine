using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using AngryWasp.Logger;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using Engine.Physics;
using Engine.Physics.Character;
using Microsoft.Xna.Framework;
using MathHelper = BepuUtilities.MathHelper;

namespace Engine.Scene
{
    public class ScenePhysics
    {
        public readonly BufferPool DefaultBufferPool;

        private ThreadDispatcher threadDispatcher;
        private Simulation simulation;
        private CharacterControllers characters;
        private CollidableProperty<PhysicsMaterial> materials;

        public Simulation Simulation => simulation;

        public bool SimulationUpdate { get; set; } = true;

        public ScenePhysics(EngineCore engine)
        {
            DefaultBufferPool = new BufferPool();
            characters = new CharacterControllers(DefaultBufferPool);
            materials = new CollidableProperty<PhysicsMaterial>();

            var targetThreadCount = System.Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            threadDispatcher = new ThreadDispatcher(targetThreadCount);

            simulation = Simulation.Create(DefaultBufferPool, new NarrowPhaseCallbacks(characters, materials),
                new PoseIntegratorCallbacks(new Vector3(0, -9.81f, 0)), new SolveDescription(8, 1));
        }

        public void Destroy()
        {
            Threading.EnsureUIThread();
            characters.Dispose();
            materials.Dispose();
            simulation.Dispose();

            DefaultBufferPool.Clear();
        }

        public BodyHandle AddDynamicBody(BodyDescription body, PhysicsMaterial material)
        {
            var handle = simulation.Bodies.Add(body);
            materials.Allocate(handle) = material;
            return handle;
        }

        public void RemoveDynamicBody(BodyHandle handle)
        {
            simulation.Bodies.Remove(handle);
            materials.CompactBodies();
        }

        public StaticHandle AddStaticBody(StaticDescription body, PhysicsMaterial material)
        {
            var handle = simulation.Statics.Add(body);
            materials.Allocate(handle) = material;
            return handle;
        }

        public void RemoveStaticBody(StaticHandle handle)
        {
            simulation.Statics.Remove(handle);
            materials.CompactStatics();
        }

        public ref CharacterController AddCharacter(BodyHandle bodyHandle) =>
            ref characters.AllocateCharacter(bodyHandle);

        public ref CharacterController GetCharacter(BodyHandle bodyHandle) =>
            ref characters.GetCharacterByBodyHandle(bodyHandle);

        public void DummyUpdate(GameTime gameTime) { }

        public void TimestepUpdate(GameTime gameTime)
        {
            simulation.Timestep(1.0f / FPSCounter.FramePerSecond, threadDispatcher);
        }

        public QuickList<SweepHit> SphericalSweep(float radius, Vector3 position, Quaternion rotation)
        {
            var hitHandler = new SweepHitHandler(DefaultBufferPool);
            simulation.Sweep(new Sphere(radius), new RigidPose(position, rotation), new BodyVelocity(), 0, DefaultBufferPool, ref hitHandler);
            return hitHandler.Hits;
        }

        public QuickList<RayHit> RayCast(Ray ray)
        {
            var hitHandler = new RayHitHandler(DefaultBufferPool);
            simulation.RayCast(ray.Position, ray.Direction, ray.MaximumT, ref hitHandler);
            return hitHandler.Hits;
        }
    }

    public struct SweepHit
    {
        public float T;
        public Vector3 Location;
        public Vector3 Normal;
        public CollidableReference Collidable;
    }

    unsafe struct SweepHitHandler : ISweepHitHandler
    {
        public QuickList<SweepHit> Hits;

        private BufferPool bufferPool;

        public SweepHitHandler(BufferPool bufferPool)
        {
            this.bufferPool = bufferPool;
            Hits = new QuickList<SweepHit>(10, bufferPool);
        }

        public bool AllowTest(CollidableReference collidable)
        {
            return collidable.Mobility == CollidableMobility.Dynamic;
        }

        public bool AllowTest(CollidableReference collidable, int child)
        {
            return collidable.Mobility == CollidableMobility.Dynamic;
        }

        public void OnHit(ref float maximumT, float t, Vector3 location, Vector3 normal, CollidableReference collidable)
        {
            maximumT = t;

            var hit = new SweepHit();
            hit.T = t;
            hit.Location = location;
            hit.Normal = normal;
            hit.Collidable = collidable;

            Hits.Add(hit, bufferPool);
        }

        public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
        {
            Debugger.Break();
        }
    }

    public struct RayHit
    {
        public float T;
        public Vector3 Normal;
        public CollidableReference Collidable;
        public int ChildIndex;
    }

    unsafe struct RayHitHandler : IRayHitHandler
    {
        public QuickList<RayHit> Hits;

        private BufferPool bufferPool;

        public RayHitHandler(BufferPool bufferPool)
        {
            this.bufferPool = bufferPool;
            Hits = new QuickList<RayHit>(10, bufferPool);
        }

        public bool AllowTest(CollidableReference collidable) => true;

        public bool AllowTest(CollidableReference collidable, int childIndex) => true;

        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            maximumT = t;

            var hit = new RayHit();
            hit.T = t;
            hit.Normal = normal;
            hit.Collidable = collidable;
            hit.ChildIndex = childIndex;

            Hits.Add(hit, bufferPool);
        }
    }

    unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public CharacterControllers Characters;
        public CollidableProperty<PhysicsMaterial> CollidableMaterials;

        public NarrowPhaseCallbacks(CharacterControllers characters, CollidableProperty<PhysicsMaterial> collidableMaterials)
        {
            Characters = characters;
            CollidableMaterials = collidableMaterials;
        }

        public void Initialize(Simulation simulation)
        {
            Characters.Initialize(simulation);
            CollidableMaterials.Initialize(simulation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin) =>
            a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            var a = CollidableMaterials[pair.A];
            var b = CollidableMaterials[pair.B];
            pairMaterial.FrictionCoefficient = a.FrictionCoefficient * b.FrictionCoefficient;
            pairMaterial.MaximumRecoveryVelocity = MathF.Max(a.MaximumRecoveryVelocity, b.MaximumRecoveryVelocity);
            pairMaterial.SpringSettings = pairMaterial.MaximumRecoveryVelocity == a.MaximumRecoveryVelocity ? a.SpringSettings : b.SpringSettings;
            Characters.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold) => true;

        public void Dispose()
        {
            Characters.Dispose();
        }
    }

    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        public float LinearDamping;
        public float AngularDamping;

        private Vector3Wide gravityWideDt;
        private Vector<float> linearDampingDt;
        private Vector<float> angularDampingDt;

        public void Initialize(Simulation simulation)
        {
        }

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving; //Don't care about fidelity in this demo!

        public bool AllowSubstepsForUnconstrainedBodies => false;

        public bool IntegrateVelocityForKinematics => false;

        public PoseIntegratorCallbacks(Vector3 gravity, float linearDamping = .03f, float angularDamping = .03f) : this()
        {
            Gravity = gravity;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
        }

        public void PrepareForIntegration(float dt)
        {
            linearDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt));
            angularDampingDt = new Vector<float>(MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt));
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear = (velocity.Linear + gravityWideDt) * linearDampingDt;
            velocity.Angular = velocity.Angular * angularDampingDt;
        }
    }
}