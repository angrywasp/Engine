using System;
using System.Collections.Generic;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using Engine.Content.Model.Instance;
using Engine.Interfaces;
using Engine.World.Components;

namespace Engine.World.Objects
{
    public class DynamicInstancingGroupManager
    {
        private EngineCore engine;
        private Dictionary<int, DynamicInstancingGroup> groups = new Dictionary<int, DynamicInstancingGroup>();
        private static AsyncLock updateLock = new AsyncLock();

        public Dictionary<int, DynamicInstancingGroup> Groups => groups;

        public DynamicInstancingGroup this[int key] => groups.ContainsKey(key) ? groups[key] : null;

        public DynamicInstancingGroupManager(EngineCore engine)
        {
            this.engine = engine;
        }

        public void Add(int key, IDrawableObjectInstanced obj)
        {
            var lck = updateLock.Lock();

            try
            {

                if (!groups.ContainsKey(key))
                {
                    MeshInstance meshInstance;

                    if (obj is MeshComponent meshComponent)
                        meshInstance = meshComponent.Mesh;
                    else if (obj is StaticMesh staticMesh)
                        meshInstance = staticMesh.Mesh;
                    else
                        throw new Exception($"Type {obj.GetType().Name} is not supported");

                    groups.TryAdd(key, DynamicInstancingGroup.Create(engine, meshInstance));
                }

                groups[key].Add(obj);

            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }

            lck.Dispose();
        }

        public void Remove(int key, IDrawableObjectInstanced obj)
        {
            if (!groups.ContainsKey(key))
                return;

            var lck = updateLock.Lock();

            groups[key].Remove(obj);

            if (groups[key].Count == 0)
                groups.Remove(key);

            lck.Dispose();
        }
    }
}