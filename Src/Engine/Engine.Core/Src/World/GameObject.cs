using Engine.Interfaces;
using Engine.Multiplayer;
using Microsoft.Xna.Framework;
using Engine.Cameras;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using AngryWasp.Logger;
using System.Collections.Generic;
using System.Numerics;
using Engine.Physics;
using Engine.World.Objects;
using Engine.Content;
using Lidgren.Network;
using System.Linq;

namespace Engine.World
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GameObjectType : IJsonSerialized
    {
        private bool isLoaded;
        private EngineCore engine;

        [JsonProperty] public Synchronization_Method SynchronizationMethod { get; set; } = Synchronization_Method.Synchronized;

        [JsonProperty] public string Controller { get; set; }

        [JsonProperty] public string PhysicsModel { get; set; }

        [JsonProperty] public List<ComponentType> Components { get; set; } = new List<ComponentType>();

        public bool IsLoaded => isLoaded;

        public virtual void Load(EngineCore engine)
        {
            this.engine = engine;
            isLoaded = true;
        }

        public virtual void Unload() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GameObject : IJsonSerialized
    {
        public enum Network_Message : ushort
        {
            Transform,
            ComponentPhysicsTransform
        }

        public event EngineEventHandler<string, Component> ComponentAddedToMap;
        public event EngineEventHandler<string, Component> ComponentRemovedFromMap;

        private GameObjectType _type = null;
        public GameObjectType Type => _type;

        protected internal EngineCore engine;
        protected internal WorldTransform3 transform = new WorldTransform3();
        private GameObjectController intellect;
        internal string name;
        internal ushort uid;
        private ChildLoaderDictionary<string, Component> loader = new ChildLoaderDictionary<string, Component>();
        private Vector3 lastSentPosition = Vector3.Zero;
        private Quaternion lastSentRotation = Quaternion.Identity;

        public WorldTransform3 Transform => transform;
        public GameObjectController Intellect => intellect;
        public string Name => name;
        public ushort UID => uid;
        public PhysicsModel PhysicsModel { get; set; } = null;
        public Dictionary<string, Component> Components { get; set; } = new Dictionary<string, Component>();

        public virtual async Task LoadAsync(EngineCore engine)
        {
            this.engine = engine;
            transform.TransformChanged += OnTransformChanged;

            loader.Run(default, default);
            LoadPhysicsModel(_type.PhysicsModel);

            if (Components.Count > 0)
                Log.Instance.WriteFatalException(new InvalidOperationException("GameObject loaded with components attached"));

            foreach (var x in _type.Components)
            {
                if (x.OnlyInEditor && !engine.IsEditor)
                    continue;

                await LoadComponentAsync(x, false).ConfigureAwait(false);
            }

            var t = _type.Controller;
            bool hasController = !string.IsNullOrEmpty(_type.Controller);
            //Assign controller
            if (hasController)
            {
                switch (engine.NetworkType)
                {
                    case Network_User_Type.Offline:
                    case Network_User_Type.Server:
                        var controller = await engine.TypeFactory.CreateGameObjectAsync<GameObjectController>(_type.Controller).ConfigureAwait(false);
                        this.AssignController(controller);
                        break;
                }
            }
        }

        public async Task<Component> LoadComponentAsync(ComponentType x, bool queuedAdd = true)
        {
            System.Type t = System.Type.GetType(x.ComponentClass);
            Component c = (Component)Activator.CreateInstance(t);

            engine.TypeFactory.SetTypeFields(t, c, x);

            await c.CreateFromTypeAsync(this).ConfigureAwait(false);

            if (queuedAdd)
                loader.QueueObjectAdd(x.Name, c);
            else
            {
                Components.Add(x.Name, c);
                c.OnAddedToMap();
                ComponentAddedToMap?.Invoke(x.Name, c);
            }

            return c;
        }

        public void UnloadComponent(string key, bool queuedRemove = true)
        {
            if (queuedRemove)
                loader.QueueObjectRemove(key);
            else
            {
                if (Components.TryGetValue(key, out Component x))
                {
                    Components.Remove(key);
                    x.OnRemovedFromMap();
                    ComponentRemovedFromMap?.Invoke(key, x);
                }
            }
        }

        private void LoadPhysicsModel(string value)
        {
            if (string.IsNullOrEmpty(_type.PhysicsModel))
                return;

            if (PhysicsModel != null && PhysicsModel.IsLoaded)
                Log.Instance.WriteFatalException(new InvalidOperationException("LoadPhysicsModel called when physics model already loaded"));

            var pm = ContentLoader.LoadJson<PhysicsModel>(value);
            pm.Load(engine);
            PhysicsModel = pm;
        }

        public virtual void Update(Camera camera, GameTime gameTime)
        {
            loader.Update(
                (ik, iv) =>
                {
                    Components.Add(ik, iv);
                    iv.OnAddedToMap();
                    ComponentAddedToMap?.Invoke(ik, iv);
                },
                (ik) =>
                {
                    Components.TryGetValue(ik, out Component iv);
                    if (iv == null)
                        return;

                    Components.Remove(ik);
                    iv.OnRemovedFromMap();
                    ComponentRemovedFromMap?.Invoke(ik, iv);
                }
            );

            PhysicsModel?.Update(gameTime);

            foreach (var x in Components.Values)
                x.Update(camera, gameTime);

            switch (engine.NetworkType)
            {
                case Network_User_Type.Offline:
                case Network_User_Type.Server:
                    {
                        if (intellect != null)
                            intellect.Update(camera, gameTime);
                    }
                    break;
            }

            switch (engine.NetworkType)
            {
                case Network_User_Type.Server:
                    /*{
                        if (Position != lastSentPosition || Rotation != lastSentRotation)
                        {
                            var msg = engine.NetworkServer.NetworkObject.CreateMessage();
                            msg.Write((ushort)Custom_Message_Type.MapObjectUpdate);
                            msg.Write(uid);
                            msg.Write((ushort)Network_Message.Transform);
                            msg.Write(Position);
                            msg.Write(Rotation);
                            engine.NetworkServer.NetworkObject.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                        }

                        lastSentPosition = Position;
                        lastSentRotation = Rotation;

                        foreach (var c in Components)
                        {
                            MeshComponent mc = c.Value as MeshComponent;
                            if (mc == null)
                                continue;

                            if (mc.Body == null)
                                continue;

                            Debugger.Break();

                            //if (mc.Body.WorldTransform != mc.LastSentTransform)
                            {
                                //todo: we can optimize this by only each MotionState value if it changes
                                var msg = engine.NetworkServer.NetworkObject.CreateMessage();
                                msg.Write((ushort)Custom_Message_Type.MapObjectUpdate);
                                msg.Write(uid);
                                msg.Write((ushort)Network_Message.ComponentPhysicsTransform);
                                msg.Write(mc.Type.Name);
                                //msg.Write(mc.Body.PosePosition);
                                //msg.Write(mc.Body.Rotation);
                                //msg.Write(mc.Body.LinearVelocity);
                                //msg.Write(mc.Body.AngularVelocity);
                                engine.NetworkServer.NetworkObject.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                            }
                            
                            //mc.LastSentTransform = mc.Body.WorldTransform;
                        }
                    }*/
                    break;
            }
        }

        public void AssignController(GameObjectController i)
        {
            this.intellect = i;
            i.AssignControlledObject(this);
        }

        public virtual void OnAddedToMap() { }

        public virtual void OnRemovedFromMap()
        {
            PhysicsModel?.Unload();

            foreach (var c in Components.ToArray())
            {
                c.Value.OnRemovedFromMap();
                Components.Remove(c.Key);
            }
        }

        public virtual void OnTransformChanged(WorldTransform3 transform) { }

        public virtual void ProcessClientMapObjectAdd(MapObject item) { }

        public virtual void ProcessMessage(ushort uid, ushort mapObjectCommand, NetIncomingMessage message)
        {
            Network_Message moc = (Network_Message)mapObjectCommand;

            switch (engine.NetworkType)
            {
                case Network_User_Type.Server:
                    /*{
                        Log.Instance.Write("Server received DynamicGameObject network message: " + moc.ToString());
                        
                        switch (moc)
                        {
                            case Network_Message.Transform:
                                {
                                    Vector3 pos = message.ReadVector3();
                                    Quaternion rot = message.ReadQuaternion();

                                    //when we call SetTransform on the server, it will send a message out to the clients to update their position
                                    SetTransform(pos, rot, Vector3.One);
                                    UpdatePhysicsTransform();

                                    var msgOut = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.MapObjectUpdate);
                                    msgOut.Write(uid);
                                    msgOut.Write((ushort)Network_Message.Transform);

                                    msgOut.Write(pos);
                                    msgOut.Write(rot);

                                    engine.NetworkServer.NetworkObject.SendToAll(msgOut, NetDeliveryMethod.ReliableOrdered);
                                }
                                break;
                            case Network_Message.ComponentPhysicsTransform:
                                {
                                    Debugger.Break(); //this shouldn't happen. we only update the physics on the server
                                }
                                break;
                        }
                    }*/
                    break;
                case Network_User_Type.Client:
                    /*{
                        Log.Instance.Write("Client received DynamicGameObject network message: " + moc.ToString());

                        switch (moc)
                        {
                            case Network_Message.Transform:
                                {
                                    Vector3 pos = message.ReadVector3();
                                    Quaternion rot = message.ReadQuaternion();

                                    SetTransform(pos, rot, Vector3.One);
                                    UpdatePhysicsTransform();
                                }
                                break;
                            case Network_Message.ComponentPhysicsTransform:
                                {
                                    string n = message.ReadString();
                                    Vector3 pos = message.ReadVector3();
                                    Quaternion rot = message.ReadQuaternion();
                                    Vector3 linVel = message.ReadVector3();
                                    Vector3 angVel = message.ReadVector3();

                                    Debugger.Break();

                                    //todo: fix this

                                    //MeshComponent mc = (MeshComponent)components[n];
                                    //mc.Body.WorldTransform = Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(pos);
                                    //mc.Body.LinearVelocity = linVel;
                                    //mc.Body.AngularVelocity = angVel;
                                }
                                break;
                        }
                    }*/
                    break;
            }
        }

        protected void ServerSendToAllClientsExceptThis(ushort uid, NetOutgoingMessage msgOut)
        {
            List<NetConnection> serverUserConnections = new List<NetConnection>();
            foreach (var user in PlayerManager.Instance.ServerUsers.Values)
            {
                if (user.MapObjectID == uid)
                    continue;

                serverUserConnections.Add(user.Peer.Connection);
            }

            if (serverUserConnections.Count > 0)
                engine.NetworkServer.NetworkObject.SendMessage(msgOut, serverUserConnections, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }
}
