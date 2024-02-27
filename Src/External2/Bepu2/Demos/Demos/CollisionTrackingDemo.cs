﻿using System;
using System.Numerics;
using BepuUtilities;
using BepuPhysics;
using DemoContentLoader;
using DemoRenderer;
using BepuPhysics.Collidables;
using DemoUtilities;
using DemoRenderer.UI;
using BepuPhysics.CollisionDetection;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using System.Runtime.CompilerServices;
using BepuPhysics.Constraints;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Demos.Demos;

/// <summary>
/// Shows how to store out collision information provided in <see cref="INarrowPhaseCallbacks"/> for later analysis, 
/// and how to use it to implement something like collision events without imposing the weird control flow that events imply.
/// </summary>
/// <remarks>
/// This implementation focuses more on simplicity than performance.
/// It's not too slow, but if you run into bottlenecks with tens of thousands of bodies with tracked collisions, be advised that you can make it faster!
/// </remarks>
public class CollisionTrackingDemo : Demo
{
    /// <summary>
    /// Refers to a collidable, or a child within a collidable.
    /// </summary>
    /// <remarks>This contact tracker allows the children of compounds to be tracked, so it's not enough to only store a <see cref="CollidableReference"/>.
    /// That would only permit top-level pair analysis.</remarks>
    public struct ContactSource : IEqualityComparerRef<ContactSource>, IEquatable<ContactSource>
    {
        /// <summary>
        /// Collidable associated with this listener.
        /// </summary>
        public CollidableReference Collidable;
        /// <summary>
        /// Child index within the collidable associated with this listener, if any. -1 if the listener is associated with the entire collidable.
        /// </summary>
        public int ChildIndex;

        public bool Equals(ref ContactSource a, ref ContactSource b) => a.Collidable == b.Collidable && a.ChildIndex == b.ChildIndex;
        public int Hash(ref ContactSource item) => (int)(item.Collidable.Packed ^ uint.RotateLeft((uint)item.ChildIndex, 16));
        public bool Equals(ContactSource other) => Equals(ref this, ref other);
        public override bool Equals(object obj) => obj is ContactSource source && Equals(source);
        public override int GetHashCode() => Hash(ref this);
        public static bool operator ==(ContactSource left, ContactSource right) => left.Equals(right);
        public static bool operator !=(ContactSource left, ContactSource right) => !(left == right);

        public static implicit operator ContactSource(CollidableReference collidable) => new() { Collidable = collidable, ChildIndex = -1 };
    }

    /// <summary>
    /// Stores contacts generated by the narrow phase for a pair.
    /// </summary>
    public struct PairCollision
    {
        /// <summary>
        /// Stores whether the 'other' collidable (the one that isn't the current tracked object) is stored in the first slot of the collision pair. True if it is, false if it isn't.
        /// This matters for interpreting the contact data: by convention, the contact offset is relative to collidable A, and the normal points from B to A.
        /// </summary>
        public bool OtherIsAInPair;

        /// <summary>
        /// Gets the contacts generated by the narrow phase in the latest update affecting this pair.
        /// </summary>
        public NonconvexContactManifold Contacts;
    }



    /// <summary>
    /// Tracks collisions for a set of collidables (or their children).
    /// </summary>
    public class CollisionTracker : IDisposable
    {
        IThreadDispatcher dispatcher;
        BufferPool pool;
        Simulation simulation;

        public CollisionTracker(BufferPool pool)
        {
            this.pool = pool;
            Tracked = new QuickDictionary<ContactSource, TrackedPairs, ContactSource>(16, pool);
        }

        public void Initialize(Simulation simulation)
        {
            //The CollisionTracker is created before the Simulation in this demo because we're passing it in as a part of the INarrowPhaseCallbacks.
            //So, we let the callbacks initialize the simulation reference!
            this.simulation = simulation;
            //Attach the callbacks to the simulation so that we can monitor the sleeping state of bodies *after* the sleeper has run, but before the narrow phase runs.
            //This ensures we have an informative view of the sleeping state of bodies.
            //(If you sample before the sleeper, you could miss out on something going to sleep; if you sample after the narrow phase,
            //you could miss out on the fact that a body was asleep beforehand and wouldn't have reported anything in the narrow phase.)
            this.simulation.Timestepper.BeforeCollisionDetection += PrepareForNextTimestep;
            this.simulation.Timestepper.CollisionsDetected += Flush;
        }

        /// <summary>
        /// Disposes the collision tracker's internal allocations and detaches its callbacks from the simulation.
        /// </summary>
        public void Dispose()
        {
            if (Tracked.Keys.Allocated)
            {
                for (int i = 0; i < Tracked.Count; ++i)
                {
                    Tracked.Values[i].Pairs.Dispose(pool);
                }
                Tracked.Dispose(pool);
                simulation.Timestepper.BeforeCollisionDetection -= PrepareForNextTimestep;
                simulation.Timestepper.CollisionsDetected -= Flush;
            }
        }


        public struct TrackedPairs
        {
            /// <summary>
            /// Holds all pairs associated with a tracked object as found in the most recent update to this pair. 
            /// Keys are the 'other' collidable in the pair- whatever collidable isn't the tracked object.
            /// </summary>
            public QuickDictionary<ContactSource, PairCollision, ContactSource> Pairs;
            /// <summary>
            /// Holds all pairs associated with a tracked object as found in the update prior to the most recent one. 
            /// Keys are the 'other' collidable in the pair- whatever collidable isn't the tracked object.
            /// </summary>
            public QuickDictionary<ContactSource, PairCollision, ContactSource> PreviousPairs;

            public TrackedPairs(BufferPool pool)
            {
                //For simplicity, we'll initialize all the contact storage with a fixed size. It'll grow over time if more collisions per tracked object are detected.
                //Some kind of compaction would be easy to add- just iterate over all tracked objects and call Compact.
                //(Or just part of them, incrementally! Probably better to avoid excess resizes!)
                Pairs = new QuickDictionary<ContactSource, PairCollision, ContactSource>(16, pool);
                PreviousPairs = new QuickDictionary<ContactSource, PairCollision, ContactSource>(16, pool);
            }


            public void Dispose(BufferPool pool)
            {
                Pairs.Dispose(pool);
                PreviousPairs.Dispose(pool);
            }
        }

        /// <summary>
        /// Maps tracked objects to the contacts associated with them.
        /// </summary>
        public QuickDictionary<ContactSource, TrackedPairs, ContactSource> Tracked;

        /// <summary>
        /// Adds a collidable (or its child) to the collision tracker. Collisions associated with it will be included for queries.
        /// </summary>
        /// <param name="target">Collidable (or child) to track.</param>
        /// <exception cref="ArgumentException">Triggered if the collidable or child is already present.</exception>
        public void Track(ContactSource target)
        {
            if (Tracked.ContainsKey(target))
                throw new ArgumentException("Object already tracked.");
            Tracked.Add(target, new TrackedPairs(pool), pool);
        }

        /// <summary>
        /// Removes a listener from the tracker.
        /// </summary>
        /// <param name="target">Collidable (or child) to untrack.</param>
        /// <exception cref="ArgumentException">Triggered if the collidable or child is not present.</exception>
        public void Untrack(ContactSource target)
        {
            if (!Tracked.ContainsKey(target))
                throw new ArgumentException("Object is not tracked.");
            Tracked.GetTableIndices(ref target, out var tableIndex, out var elementIndex);
            Tracked.Values[elementIndex].Dispose(pool);
            Tracked.FastRemove(tableIndex, elementIndex);
        }

        //The INarrowPhaseCallbacks are invoked from multiple threads and are performance sensitive.
        //We want to do as little as possible, so rather than locking and trying to modify the final storage directly,
        //just create per-worker caches that get flushed at the end.

        /// <summary>
        /// Stores a single tracked result in a worker cache to later be flushed.
        /// </summary>
        struct WorkerPairContacts
        {
            public ContactSource Self;
            public ContactSource Other;
            public PairCollision Collision;
        }

        /// <summary>
        /// Stores contacts for listeners reported by callbacks to a particular thread.
        /// </summary>
        Buffer<QuickList<WorkerPairContacts>> workerCaches;

        /// <summary>
        /// Prepares the collision tracker for the next timestep by swapping/clearing pairs that expect updates in the coming narrow phase step and preparing the per-worker contact caches.
        /// </summary>
        void PrepareForNextTimestep(float dt, IThreadDispatcher dispatcher)
        {
            //Flip the caches for each listener and clear out the now-current set for accumulation.
            //Note that we have to do some special stuff in the case of sleeping.
            //This complexity arises from the fact that collision pairs between sleeping bodies do not invoke narrow phase callbacks, because they are never examined.
            //The broad phase will only ever emit pair tests for pairs that contain at least one wakeful body.

            //Note that this function is attached to simulation.Timestepper.BeforeCollisionDetection.
            //That fires *after* the sleeper runs and before the narrowphase, so we get an accurate picture of what pairs will end up providing contacts during the upcoming tests.
            for (int i = 0; i < Tracked.Count; ++i)
            {
                ref var tracked = ref Tracked.Values[i];
                var collidable = Tracked.Keys[i].Collidable;
                if (collidable.Mobility != CollidableMobility.Static && simulation.Bodies[collidable.BodyHandle].Awake)
                {
                    //There is no need to consider the sleeping state of the other body when the tracked object is awake; flip the buffers and clear the new front buffer.
                    BepuPhysics.Helpers.Swap(ref tracked.Pairs, ref tracked.PreviousPairs);
                    tracked.Pairs.FastClear();
                }
                else
                {
                    //The tracked object is either sleeping or static.
                    //Any pair involving a wakeful body must be flipped and cleared.
                    //Any pair that involves no wakeful bodies should preserve the old values, because pairs with no wakeful bodies did not receive new contacts in the narrow phase.

                    // NOTE: While this is not a zero cost operation, you could make the choice to ignore reported contacts for sleeping tracked bodies, 
                    //       and then just don't touch any of the contact data for sleeping tracked bodies during this phase!
                    //       That would technically miss out on reported contacts that don't result in waking up the shape.
                    //
                    //       You could also make the behavior around sleeping different on a per tracked object level.
                    //       For example, you could include extra data you want in the TrackedPairs type to decide how to handle sleeping.
                    //       This demo just does the most frequently desired thing with no configuration for simplicity's sake.
                    for (int j = tracked.Pairs.Count - 1; j >= 0; --j)
                    {
                        var other = tracked.Pairs.Keys[j];
                        if (other.Collidable.Mobility != CollidableMobility.Static && simulation.Bodies[other.Collidable.BodyHandle].Awake)
                        {
                            //Wakeful body! Push the current values into the previous buffer and remove the entry from the current.
                            tracked.PreviousPairs.AddAndReplace(other, tracked.Pairs.Values[j], pool);
                            tracked.Pairs.FastRemove(other);
                        }
                        //If this is a sleeping body or static, then the pair has no wakeful bodies and "preserving the old values" means doing nothing.
                    }
                }
            }

            const int initialWorkerCapacity = 512;
            if (dispatcher != null)
            {
                //Cache the dispatcher so ReportContacts can use the buffers in the narrowphase callbacks.
                workerCaches = new Buffer<QuickList<WorkerPairContacts>>(dispatcher.ThreadCount, pool);
                for (int i = 0; i < workerCaches.Length; ++i)
                {
                    workerCaches[i] = new QuickList<WorkerPairContacts>(initialWorkerCapacity, dispatcher.WorkerPools[i]);
                }
                this.dispatcher = dispatcher;
            }
            else
            {
                //The simulation is being updated without a thread dispatcher, so all allocations can use the main pool.
                workerCaches = new Buffer<QuickList<WorkerPairContacts>>(1, pool);
                workerCaches[0] = new QuickList<WorkerPairContacts>(initialWorkerCapacity, pool);
            }
        }

        void FlushWorkerCache(ref QuickList<WorkerPairContacts> cache, BufferPool pool)
        {
            for (int j = 0; j < cache.Count; ++j)
            {
                ref var entry = ref cache[j];
                Tracked.GetTableIndices(ref entry.Self, out _, out var elementIndex);
                Tracked.Values[elementIndex].Pairs.Add(entry.Other, entry.Collision, pool);
            }
            //The worker cache memory must be returned to the thread pool, not the main pool!
            cache.Dispose(pool);
        }
        /// <summary>
        /// Flushes all collisions found in the previous timestep into efficient storage for queries.
        /// </summary>
        void Flush(float dt, IThreadDispatcher threadDispatcher)
        {
            if (dispatcher != null)
            {
                //Flush the worker caches into the main storage and dispose the caches.
                for (int i = 0; i < workerCaches.Length; ++i)
                {
                    FlushWorkerCache(ref workerCaches[i], dispatcher.WorkerPools[i]);
                }
            }
            else
            {
                //Simulation is running sequentially and there's only one thread; the allocations are on the main pool.
                FlushWorkerCache(ref workerCaches[0], pool);
            }
            workerCaches.Dispose(pool);
            this.dispatcher = null;
        }

        private void ReportContacts<TContacts>(int workerIndex, ContactSource self, ContactSource other, bool otherIsA, ref TContacts contacts) where TContacts : unmanaged, IContactManifold<TContacts>
        {
            if (Tracked.ContainsKey(self))
            {
                //A is a listener, add it.            
                ref var pairContacts = ref workerCaches[workerIndex].Allocate(dispatcher != null ? dispatcher.WorkerPools[workerIndex] : pool);
                pairContacts.Self = self;
                pairContacts.Other = other;
                pairContacts.Collision.OtherIsAInPair = otherIsA;
                if (typeof(TContacts) == typeof(ConvexContactManifold)) //This is a JIT-time constant.
                {
                    //The pairContacts representation just uses a NonconvexContactManifold for simplicity- we can just rearrange convex contacts to fit.
                    //(Without doing this, the post-analysis would constantly have to check an "IsConvex" flag and so forth. It would be quite stinky.)
                    ref var convex = ref Unsafe.As<TContacts, ConvexContactManifold>(ref contacts);
                    for (int i = 0; i < contacts.Count; ++i)
                    {
                        ref var targetContact = ref Unsafe.Add(ref pairContacts.Collision.Contacts.Contact0, i);
                        convex.GetContact(i, out targetContact.Offset, out targetContact.Normal, out targetContact.Depth, out targetContact.FeatureId);
                    }
                    pairContacts.Collision.Contacts.Count = contacts.Count;
                    pairContacts.Collision.Contacts.OffsetB = convex.OffsetB;
                }
                else
                {
                    pairContacts.Collision.Contacts = Unsafe.As<TContacts, NonconvexContactManifold>(ref contacts);
                }
            }
        }

        /// <summary>
        /// Notifies the tracker of a collision between collidable children.
        /// </summary>
        /// <param name="collidableA">First collidable in the pair.</param>
        /// <param name="childIndexA">Child index associated with the first collidable in the pair.</param>
        /// <param name="collidableB">Second collidable in the pair.</param>
        /// <param name="childIndexB">Child index associated with the second collidable in the pair.</param>
        /// <param name="workerIndex">Index of the worker invoking this callback.</param>
        /// <param name="contacts">Contacts reported in the callback.</param>
        public void ReportChildContacts(CollidableReference collidableA, int childIndexA, CollidableReference collidableB, int childIndexB, int workerIndex, ref ConvexContactManifold contacts)
        {
            Debug.Assert(workerCaches.Allocated, "The worker caches must be allocated in order to report contacts. Make sure PrepareForNextTimestep was called.");
            var a = new ContactSource { Collidable = collidableA, ChildIndex = childIndexA };
            var b = new ContactSource { Collidable = collidableB, ChildIndex = childIndexB };
            ReportContacts(workerIndex, a, b, false, ref contacts);
            ReportContacts(workerIndex, b, a, true, ref contacts);
        }


        /// <summary>
        /// Notifies the tracker of a collision between collidables.
        /// </summary>
        /// <typeparam name="TManifold">Type of the manifold being reported.</typeparam>
        /// <param name="collidableA">First collidable in the pair.</param>
        /// <param name="collidableB">Second collidable in the pair.</param>
        /// <param name="workerIndex">Index of the worker invoking this callback.</param>
        /// <param name="contacts">Contacts reported in the callback.</param>
        public void ReportContacts<TManifold>(CollidableReference collidableA, CollidableReference collidableB, int workerIndex, ref TManifold contacts) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            Debug.Assert(workerCaches.Allocated, "The worker caches must be allocated in order to report contacts. Make sure PrepareForNextTimestep was called.");
            Debug.Assert(workerCaches.Allocated, "The worker caches must be allocated in order to report contacts. Make sure PrepareForNextTimestep was called.");
            var a = new ContactSource { Collidable = collidableA, ChildIndex = -1 };
            var b = new ContactSource { Collidable = collidableB, ChildIndex = -1 };
            ReportContacts(workerIndex, a, b, false, ref contacts);
            ReportContacts(workerIndex, b, a, true, ref contacts);
        }

    }

    /// <summary>
    /// Callbacks invoked by the simulation's narrow phase.
    /// In this demo, we'll collect all contact data associated with tracked objects for later processing.
    /// </summary>
    public unsafe struct CollisionTrackingCallbacks : INarrowPhaseCallbacks
    {
        CollisionTracker collisionTracker;
        public CollisionTrackingCallbacks(CollisionTracker collisionTracker)
        {
            this.collisionTracker = collisionTracker;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 1f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            collisionTracker.ReportContacts(pair.A, pair.B, workerIndex, ref manifold);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            collisionTracker.ReportChildContacts(pair.A, childIndexA, pair.B, childIndexB, workerIndex, ref manifold);
            return true;
        }

        public void Initialize(Simulation simulation)
        {
            collisionTracker.Initialize(simulation);
        }

        public void Dispose()
        {
        }

    }

    /// <summary>
    /// Tracks a particle created by a collision.
    /// </summary>
    struct ContactResponseParticle
    {
        public Vector3 Position;
        public float Age;
        public Vector3 Normal;
    }
    QuickList<ContactResponseParticle> particles;

    CollisionTracker collisionTracker;


    public override void Initialize(ContentArchive content, Camera camera)
    {
        camera.Position = new Vector3(0, 8, -20);
        camera.Yaw = MathHelper.Pi;

        particles = new QuickList<ContactResponseParticle>(8, BufferPool);

        collisionTracker = new CollisionTracker(BufferPool);
        Simulation = Simulation.Create(BufferPool, new CollisionTrackingCallbacks(collisionTracker), new DemoPoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));


        var listenedBody1 = Simulation.Bodies.Add(BodyDescription.CreateConvexDynamic(new Vector3(0, 5, 0), 1, Simulation.Shapes, new Box(1, 2, 3)));
        collisionTracker.Track(Simulation.Bodies[listenedBody1].CollidableReference);

        var listenedBody2 = Simulation.Bodies.Add(BodyDescription.CreateConvexDynamic(new Vector3(0.5f, 10, 0), 1, Simulation.Shapes, new Capsule(0.25f, 0.7f)));
        collisionTracker.Track(Simulation.Bodies[listenedBody2].CollidableReference);


        Simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), Simulation.Shapes.Add(new Box(30, 1, 30))));
        Simulation.Statics.Add(new StaticDescription(new Vector3(0, 3, 15), Simulation.Shapes.Add(new Box(30, 5, 1))));
    }

    void AddParticle(Vector3 contactOffset, Vector3 contactNormal, CollidableReference firstCollidableInPair)
    {
        ref var particle = ref particles.Allocate(BufferPool);
        //Contact data is calibrated according to the order of the pair, so using A's position is important.
        particle.Position = contactOffset + (firstCollidableInPair.Mobility == CollidableMobility.Static ?
            new StaticReference(firstCollidableInPair.StaticHandle, Simulation.Statics).Pose.Position :
            new BodyReference(firstCollidableInPair.BodyHandle, Simulation.Bodies).Pose.Position);
        particle.Age = 0;
        particle.Normal = contactNormal;
    }

    bool PreviousContainsTouchingFeatureId(ref PairCollision pair, int featureId)
    {
        for (int i = 0; i < pair.Contacts.Count; ++i)
        {
            if (pair.Contacts.GetFeatureId(i) == featureId && pair.Contacts.GetDepth(ref pair.Contacts, i) >= 0)
                return true;
        }
        return false;
    }

    public override void Update(Window window, Camera camera, Input input, float dt)
    {
        //base.Update includes a call to the Simulation.Timestep.
        base.Update(window, camera, input, dt);

        //Now analyze the contacts we've collected. We'll match the ContactEventsDemo's OnContactAdded behavior.
        //Note that you could implement any query, including queries that span multiple contact manifolds and children.
        //Having all of the contact state accessible at once tends to provide a lot more flexibility than events,
        //and it tends to be easier to reason about the control flow.
        //(The analysis is exactly where you do it, not inside some other thread's execution in the middle of the physics engine!)
        for (int trackedIndex = 0; trackedIndex < collisionTracker.Tracked.Count; ++trackedIndex)
        {
            ref var collisions = ref collisionTracker.Tracked.Values[trackedIndex];
            var self = collisionTracker.Tracked.Keys[trackedIndex];
            for (int pairIndex = 0; pairIndex < collisions.Pairs.Count; ++pairIndex)
            {
                ref var pair = ref collisions.Pairs.Values[pairIndex];
                var other = collisions.Pairs.Keys[pairIndex];
                if (collisions.PreviousPairs.GetTableIndices(ref other, out _, out int otherIndexInPrevious))
                {
                    //There exists a previous collision.
                    ref var previous = ref collisions.PreviousPairs.Values[otherIndexInPrevious];
                    for (int i = 0; i < pair.Contacts.Count; ++i)
                    {
                        if (pair.Contacts.GetDepth(ref pair.Contacts, i) >= 0)
                        {
                            //This contact is touching. Does there exist a contact with the same feature id that was touching in the previous timestep?
                            if (!PreviousContainsTouchingFeatureId(ref previous, pair.Contacts.GetFeatureId(i)))
                                AddParticle(pair.Contacts.GetOffset(ref pair.Contacts, i), pair.Contacts.GetNormal(ref pair.Contacts, i), pair.OtherIsAInPair ? other.Collidable : self.Collidable);
                        }
                    }
                }
                else
                {
                    //No previous collision, so all contacts are new.
                    for (int i = 0; i < pair.Contacts.Count; ++i)
                    {
                        AddParticle(pair.Contacts.GetOffset(ref pair.Contacts, i), pair.Contacts.GetNormal(ref pair.Contacts, i), pair.OtherIsAInPair ? other.Collidable : self.Collidable);
                    }
                }
            }
        }

        //Age and scoot the particles we created for new contacts for the animation.
        for (int i = particles.Count - 1; i >= 0; --i)
        {
            ref var particle = ref particles[i];
            particle.Age += dt;
            if (particle.Age > 0.7325f)
            {
                particles.FastRemoveAt(i);
            }
            else
            {
                particle.Position += particle.Normal * (2 * dt);
            }

        }
    }

    public override void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
    {
        for (int i = particles.Count - 1; i >= 0; --i)
        {
            ref var particle = ref particles[i];
            var radius = particle.Age * (particle.Age * (0.135f - 2.7f * particle.Age) + 1.35f);
            var pose = new RigidPose(particle.Position);
            renderer.Shapes.AddShape(new Sphere(radius), Simulation.Shapes, pose, new Vector3(0, 1, 0));
        }

        var resolution = renderer.Surface.Resolution;
        renderer.TextBatcher.Write(text.Clear().Append("Collision events aren't the only way to interact with contacts!"), new Vector2(16, resolution.Y - 80), 16, Vector3.One, font);
        renderer.TextBatcher.Write(text.Clear().Append("This demo collects contacts in the INarrowPhaseCallbacks implementation and hands them off to a CollisionTracker type."), new Vector2(16, resolution.Y - 64), 16, Vector3.One, font);
        renderer.TextBatcher.Write(text.Clear().Append("Tracked collidables keep the current and previous collision state around."), new Vector2(16, resolution.Y - 48), 16, Vector3.One, font);
        renderer.TextBatcher.Write(text.Clear().Append("This collision state can be queried after the fact to serve many of the same use cases as collision events,"), new Vector2(16, resolution.Y - 32), 16, Vector3.One, font);
        renderer.TextBatcher.Write(text.Clear().Append("but in a more flexible way that doesn't force the user to think about potentially confusing execution contexts and control flows."), new Vector2(16, resolution.Y - 16), 16, Vector3.One, font);

        base.Render(renderer, camera, input, text, font);
    }
}
