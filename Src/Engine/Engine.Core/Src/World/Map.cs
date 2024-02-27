using Engine.Content;
using Engine.Multiplayer;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Helpers;
using Engine.Cameras;
using AngryWasp.Logger;
using Engine.Physics;
using System.Numerics;
using Engine.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Engine.World.Objects;

namespace Engine.World
{
    //todo: need shadow casting for static meshes
    [JsonObject(MemberSerialization.OptIn)]
    public class Map : IJsonSerialized
    {
        public enum Network_Message : ushort
        {
            ObjectAddedToQueue,
            ObjectAddedToScene,
            ObjectRemovedFromScene,
        }

        public event EngineEventHandler<string, MapObject> ObjectAddedToMap;
        public event EngineEventHandler<string, MapObject> ObjectRemovedFromMap;

        [JsonProperty] public Vector3 EditorCameraPosition { get; set; } = Vector3.Zero;
        [JsonProperty] public float EditorCameraYaw { get; set; } = 0;
        [JsonProperty] public float EditorCameraPitch { get; set; } = 0;
        [JsonProperty] public List<MapObject> Objects { get; set; } = new List<MapObject>();
        [JsonProperty] public List<StaticMesh> StaticMeshes { get; set; } = new List<StaticMesh>();
        [JsonProperty] public string PhysicsModelPath { get; set; } = null;

        private EngineCore engine;
        private ushort uidIter = 0;
        private ChildLoaderDictionary<ushort, MapObject> loader = new ChildLoaderDictionary<ushort, MapObject>();

        public static List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
        public PhysicsModel PhysicsModel { get; set; } = new PhysicsModel();

        public void Update(Camera camera, GameTime gameTime)
        {
            loader.Update(
                (ik, iv) =>
                {
                    Objects.Add(iv);
                    iv.OnAddedToMap();
                    ObjectAddedToMap?.Invoke(iv.Name, iv);
                },
                (ik) =>
                {
                    var iv = FindByNetworkUID(ik);
                    if (iv == null)
                        return;

                    Objects.Remove(iv);
                    iv.OnRemovedFromMap();
                    ObjectRemovedFromMap?.Invoke(iv.Name, iv);
                }
            );
            
            foreach (StaticMesh sm in StaticMeshes)
                sm.Update(camera, gameTime);

            foreach (MapObject item in Objects)
                item.Update(camera, gameTime);
        }

        private int itemsToLoad = 0;
        private int itemsLoaded = 0;

        public int ObjectCount => Objects.Count + StaticMeshes.Count + 1; //for physics model

        public async Task LoadAsync(EngineCore engine, Action<int> loadingScreenCallback, Action<Map> loadedCallback)
        {
            try
            {
                this.engine = engine;
                itemsToLoad = ObjectCount;

                loader.Run(
                    async (addList) =>
                    {
                        switch (engine.NetworkType)
                        {
                            case Network_User_Type.Offline:
                                {
                                    foreach (var i in addList)
                                        await i.Value.LoadAsync(engine).ConfigureAwait(false);
                                }
                                break;
                            case Network_User_Type.Server:
                                {
                                    foreach (var i in addList)
                                    {
                                        await i.Value.LoadAsync(engine).ConfigureAwait(false);

                                        //we have already sent everything to the client to create the map object.
                                        //this is just a message telling clients the server has added it to the scene
                                        //so we just send the networkUid and the client will find it on their side
                                        var msg = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.Map);
                                        msg.Write((ushort)Map.Network_Message.ObjectAddedToScene);
                                        msg.Write(i.Key);

                                        engine.NetworkServer.NetworkObject.SendToAll(msg, Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                                    }
                                }
                                break;
                            case Network_User_Type.Client:
                                {
                                    foreach (var i in addList)
                                    {
                                        await i.Value.LoadAsync(engine).ConfigureAwait(false);

                                        foreach (var o in Objects)
                                        {
                                            if (addList.ContainsKey(o.UID))
                                                continue;

                                            o.GameObject.ProcessClientMapObjectAdd(i.Value);
                                        }
                                    }
                                }
                                break;
                        }
                    },
                    async (removeList) =>
                    {
                        foreach (var i in removeList)
                        {
                            var iv = FindByNetworkUID(i);
                            if (iv == null)
                                return;

                            //iv.Unload();

                            if (engine.NetworkType == Network_User_Type.Server)
                            {
                                var msg = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.Map);
                                msg.Write((ushort)Map.Network_Message.ObjectRemovedFromScene);
                                msg.Write(i);

                                engine.NetworkServer.NetworkObject.SendToAll(msg, Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                            }
                        }
                    }
                );

                foreach (var item in Objects)
                {
                    item.uid = ++uidIter;
                    item.Loaded += (mapObject, gameObject) => { HandleObjectLoaded(loadingScreenCallback, loadedCallback); };
                    await item.LoadAsync(engine).ConfigureAwait(false);
                    item.OnAddedToMap();
                    ObjectAddedToMap?.Invoke(item.Name, item);
                }

                foreach (StaticMesh sm in StaticMeshes)
                {
                    sm.Loaded += (staticMesh) => { HandleObjectLoaded(loadingScreenCallback, loadedCallback); };
                    sm.LoadAsync(engine);
                }

                if (!string.IsNullOrEmpty(PhysicsModelPath))
                    PhysicsModel = ContentLoader.LoadJson<PhysicsModel>(PhysicsModelPath);

                PhysicsModel.Loaded += (physicsModel) => { HandleObjectLoaded(loadingScreenCallback, loadedCallback); };
                PhysicsModel.Load(engine);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private void HandleObjectLoaded(Action<int> loadingScreenCallback, Action<Map> loadedCallback)
        {
            ++itemsLoaded;
            loadingScreenCallback(itemsLoaded);
            if (itemsLoaded >= itemsToLoad)
                loadedCallback(this);
        }

        public void Unload()
        {
            Log.Instance.Write("TODO: Need map object unloading");
            Objects.Clear();
        }

        public void QueueObjectAdd(MapObject mapObject) => loader.QueueObjectAdd(mapObject.UID, mapObject);

        public void QueueObjectRemove(ushort uid) => loader.QueueObjectRemove(uid);

        public MapObject FindByNetworkUID(ushort uid)
        {
            foreach (var item in Objects)
                if (item.UID == uid)
                    return item;

            return null;
        }

        public MapObject FindByName(string name)
        {
            foreach (var item in Objects)
                if (item.Name == name)
                    return item;

            return null;
        }

        public void ServerAddClientPlayerToMap(ServerConnectedPeer client)
        {
            if (engine.NetworkType != Network_User_Type.Server || PlayerManager.Instance == null)
                Debugger.Break(); //only for servers & the map must have a playermanager

            PlayerManager.Instance.QueuePlayerAdd(client);
        }

        public ushort GenerateRandomUID()
        {
            ushort test = (ushort)engine.Random.Next(0, ushort.MaxValue);

            while (FindByNetworkUID(test) != null)
                test = (ushort)engine.Random.Next(0, ushort.MaxValue);

            return test;
        }

        public string GenerateRandomName(string s)
        {
            int i = 0;
            string test = s;

            while (FindByName(test) != null)
                test = $"{s}_{i++}";

            return test;
        }

        public void GetObjects(BoundingSphere sphere, Action<MapObject> hitAction)
        {
            foreach (var obj in Objects)
            {
                if (sphere.Contains(obj.GameObject.Transform.Translation) == ContainmentType.Contains)
                {
                    hitAction(obj);
                    break;
                }
            }
        }

        private Dictionary<ushort, MapObject> clientQueue = new Dictionary<ushort, MapObject>();

        internal void ProcessMessage(ushort mapCommand, Lidgren.Network.NetIncomingMessage message)
        {
            Network_Message messageType = (Network_Message)mapCommand;

            Log.Instance.Write("Received Map network message: " + messageType.ToString());

            switch (messageType)
            {
                case Network_Message.ObjectAddedToQueue:
                    {
                        //when we get this message, it means the server has created a map object and added it to it's 
                        //add queue for processing. so on the client, we should do the same. 
                        //we create a map object and add it to clientQueue
                        //then when we get an ObjectAddedToScene message we process that list
                        //and add them to the client maps add queue. this means that
                        //a map object is always added to the client scene after the server, even though 
                        //they will load at different times

                        string gameObjectPath = message.ReadString();
                        string name = message.ReadString();
                        ushort uid = message.ReadUInt16();
                        Vector3 pos = message.ReadVector3();
                        Quaternion rot = message.ReadQuaternion();
                        Vector3 scl = message.ReadVector3();

                        MapObject mo = engine.Scene.CreateMapObject(name, gameObjectPath, uid, pos, rot, scl);
                        clientQueue.Add(uid, mo);
                    }
                    break;
                case Network_Message.ObjectAddedToScene:
                    {
                        ushort uid = message.ReadUInt16();

                        if (!clientQueue.ContainsKey(uid))
                            return;

                        loader.QueueObjectAdd(uid, clientQueue[uid]);
                        clientQueue.Remove(uid);
                    }
                    break;
                case Network_Message.ObjectRemovedFromScene:
                    {
                        ushort uid = message.ReadUInt16();

                        if (!clientQueue.ContainsKey(uid))
                            return;

                        loader.QueueObjectRemove(uid);
                    }
                    break;
            }
        }

        private bool noPointWarningWritten = false;

        public SpawnPoint GetRandomSpawnPoint()
        {
            if (SpawnPoints.Count == 0)
                return null;

            return SpawnPoints[engine.Random.Next(0, SpawnPoints.Count)];
        }

        public SpawnPoint GetFreeRandomSpawnPoint()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnPoint sp = GetRandomSpawnPoint();

                #region Check if there are spawn points

                if (sp == null)
                {
                    if (!noPointWarningWritten)
                    {
                        Log.Instance.WriteWarning("No spawn points");
                        noPointWarningWritten = true;
                    }

                    return null;
                }

                #endregion

                var hits = engine.Scene.Physics.SphericalSweep(1, sp.Transform.Translation, sp.Transform.Rotation);

                if (hits.Count == 0)
                    return sp;
            }

            return null;
        }
    }
}
