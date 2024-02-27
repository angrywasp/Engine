using AngryWasp.Helpers;
using AngryWasp.Logger;
using BepuPhysics;
using BepuPhysics.Collidables;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace Engine.Physics
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PhysicsModel : IJsonSerialized
    {
        public event EngineEventHandler<PhysicsModel> Loaded;
        public event EngineEventHandler<PhysicsModel> Unloaded;

        public event EngineEventHandler<PhysicsModel, IConvexShape> ShapeAdded;
        public event EngineEventHandler<PhysicsModel, IConvexShape> ShapeRemoved;

        private static AsyncLock updateLock = new AsyncLock();

        private EngineCore engine;

        [JsonProperty] public List<IConvexShape> Shapes { get; set; } = new List<IConvexShape>();
        [JsonProperty] public List<IConstraint> Constraints { get; set; } = new List<IConstraint>();

        public void Update(GameTime gameTime)
        {
            if (!isLoaded)
                return;

            foreach (var b in Shapes)
                b.Update();
        }

        private bool isLoaded = false;

        public bool IsLoaded => isLoaded;

        public void Unload()
        {
            foreach (var c in Constraints)
                c.RemoveFromScene();

            foreach (var b in Shapes)
                b.RemoveFromScene();

            isLoaded = false;
            Unloaded?.Invoke(this);
            Log.Instance.Write("PhysicsModel unloaded");
        }

        public void Load(EngineCore engine)
        {
            this.engine = engine;

            foreach (var b in Shapes)
                b.AddToScene(engine);

            foreach (var c in Constraints)
                c.AddToScene(engine, Shapes);

            isLoaded = true;
            Loaded?.Invoke(this);
            Log.Instance.Write("PhysicsModel loaded");
        }

        public void AddShape(IConvexShape shape)
        {
            Shapes.Add(shape);
            ShapeAdded?.Invoke(this, shape);
        }

        public void AddConstraint(IConstraint constraint) => Constraints.Add(constraint);

        public IConvexShape GetBodyForReference(CollidableReference handle)
        {
            foreach (var b in Shapes)
            {
                if (b.IsDynamic)
                {
                    if (b.BodyReference.CollidableReference == handle)
                        return b;
                }
                else if (b.StaticReference.CollidableReference == handle)
                    return b;
            }

            return null;
        }

        public void RemoveShape(IConvexShape shape)
        {
            Shapes.Remove(shape);
            shape.RemoveFromScene();
            ShapeRemoved?.Invoke(this, shape);
        }

        public void EditorUpdate(Matrix4x4 transform)
        {
            var l = updateLock.Lock();
            foreach (var shape in Shapes)
            {
                var bodyTransform = Matrix4x4.CreateFromQuaternion(shape.Pose.Rotation) * Matrix4x4.CreateTranslation(shape.Pose.Position);
                var m = bodyTransform * transform;
                Matrix4x4.Decompose(m, out _, out Quaternion rotation, out Vector3 translation);

                var newPose = new Pose(translation, rotation);

                if (shape.IsDynamic)
                {
                    shape.BodyReference.GetDescription(out BodyDescription description);
                    description.Pose = newPose.ToRigidPose();
                    shape.BodyReference.ApplyDescription(description);
                }
                else
                {
                    shape.StaticReference.GetDescription(out StaticDescription description);
                    description.Pose = newPose.ToRigidPose();
                    shape.StaticReference.ApplyDescription(description);
                }
            }

            l.Dispose();
        }
    }
}
