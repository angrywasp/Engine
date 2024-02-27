using Microsoft.Xna.Framework;
using Engine.Cameras;
using System.Numerics;
using Newtonsoft.Json;
using Engine.Physics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using System;
using System.Diagnostics;

namespace Engine.World
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ComponentType
    {
        public virtual string ComponentClass => "Engine.World.Component";

        [JsonProperty] public bool OnlyInEditor { get; set; }

        [JsonProperty] public WorldTransform3 LocalTransform { get; set; } = new WorldTransform3();

        [JsonProperty] public string Name { get; set; }

        [JsonProperty] public int PhysicsBodyIndex { get; set; } = -1;

        [JsonProperty] public string BoneName { get; set; } = null;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Component
    {
        private ComponentType _type = null;

        public ComponentType Type => _type;
        
        protected GameObject parent;
        private IConvexShape body;
        protected WorldTransform3 localTransform = new WorldTransform3();
        protected WorldTransform3 globalTransform = new WorldTransform3();
        
        public WorldTransform3 LocalTransform => localTransform;
        public WorldTransform3 GlobalTransform => globalTransform;

        public virtual async Task CreateFromTypeAsync(GameObject parent)
        {
            try
            {
                this.parent = parent;

                localTransform.TransformChanged += OnLocalTransformChanged;
                globalTransform.TransformChanged += OnGlobalTransformChanged;

                localTransform.Update(_type.LocalTransform.Matrix);
                AssociatePhysicsBody(_type.PhysicsBodyIndex);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        public void AssociatePhysicsBody(int pbi)
        {
            if (parent.PhysicsModel == null || pbi < 0 || pbi >= parent.PhysicsModel.Shapes.Count)
            {
                body = null;
                return;
            }

            body = parent.PhysicsModel.Shapes[pbi];
        }

        public virtual void Update(Camera camera, GameTime gameTime)
        {
            if (body != null)
                this.globalTransform.Update(localTransform.Matrix * body.SimulationTransform.Matrix);
            else
                this.globalTransform.Update(localTransform.Matrix * parent.Transform.Matrix);
        }

        protected virtual void OnLocalTransformChanged(WorldTransform3 localTransform)
        {
            globalTransform.Update(localTransform.Matrix * parent.Transform.Matrix);
        }

        protected virtual void OnGlobalTransformChanged(WorldTransform3 globalTransform) { }

        public virtual void OnAddedToMap() => Log.Instance.Write($"Component {_type.Name} added to map");

        public virtual void OnRemovedFromMap()
        {
            if (body != null)
            {
                parent.PhysicsModel.RemoveShape(body);
                body = null;
            }

            Log.Instance.Write($"Component {_type.Name} removed from map");
        }
    }
}
