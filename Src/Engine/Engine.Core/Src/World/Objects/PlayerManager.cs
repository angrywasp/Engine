using Engine.Multiplayer;
using Engine.World.Objects.Users;
using System.Collections.Generic;
using System.Diagnostics;
using Lidgren.Network;
using Engine.Cameras;
using Engine.Helpers;
using AngryWasp.Logger;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Engine.World.Objects
{
    public class PlayerManagerType : GameObjectType
    {
        [JsonProperty] public string PlayerUnit { get; set; }
        
        [JsonProperty] public string PlayerController { get; set; }
    }

    public class PlayerManager : GameObject
    {
        //public delegate void PlayerManagerAvatarCreatedEventHandler(MapObject avatar, MapObject controller);
        //public event PlayerManagerAvatarCreatedEventHandler AvatarCreated;

        public event EngineEventHandler<MapObject, MapObject> AvatarCreated;


        //this is a list of users that have told the server they are
        //ready to play and have been approved by the server to enter the game
        private Dictionary<short, ServerUser> serverUsers = new Dictionary<short, ServerUser>();
        private Dictionary<short, ClientUser> clientUsers = new Dictionary<short, ClientUser>();
        private OfflineUser offlineUser;

        //queue of users to add. on each update we process the users list
        private List<ServerUser> addQueue = new List<ServerUser>();
        private List<short> removeQueue = new List<short>();

        public Dictionary<short, ServerUser> ServerUsers => serverUsers;

        public enum Network_Message : ushort
        {
            ClientAddedToScene,
            ClientLoaded,
            ClientPlayerConnected,
            ClientPlayerDisconnected,
            RequestPlayerList,
            RequestPlayerListResponse
        }

        #region There must be one and can only be one in each map

        private static PlayerManager instance;
        public static PlayerManager Instance => instance;

        #endregion

        #region Required in every script class

        private PlayerManagerType _type = null;

        public new PlayerManagerType Type => _type;

        #endregion

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine);

            if (instance != null)
                Debugger.Break();

            instance = this;

            if (engine.NetworkType == Network_User_Type.Server)
                engine.NetworkServer.Disconnected += NetworkServer_Disconnected;
        }

        void NetworkServer_Disconnected(ServerConnectedPeer sender)
        {
            QueuePlayerRemove(sender);
        }

        public override void ProcessClientMapObjectAdd(MapObject item)
        {
            base.ProcessClientMapObjectAdd(item);

            foreach (var c in clientUsers.Values)
            {
                if (c.MapObjectID == item.UID)
                {
                    if (c.Peer.Id == engine.NetworkClient.ThisUser.Id)
                    {
                        //an object added to the scene is this clients avatar
                        //assign a copy of the controllerType to the map object and
                        //fire an event to let the client know they have an avatar ready and can play

                        c.Avatar = engine.Scene.Map.FindByNetworkUID(item.UID);

                        var controllerMapObject = engine.Scene.CreateMapObject(c.Peer.UserName + "Controller", _type.PlayerController);
                        controllerMapObject.Loaded += (mapObject, gameObject) =>
                        {
                            c.Avatar.GameObject.AssignController((GameObjectController)controllerMapObject.GameObject);
                            AvatarCreated?.Invoke(c.Avatar, controllerMapObject);
                            engine.Scene.Map.QueueObjectAdd(controllerMapObject);
                        };

                        controllerMapObject.LoadAsync(engine);
                    }
                }
            }
        }

        /*public override void Unload()
        {
            base.Unload();

            instance = null;
        }*/

        public override void Update(Camera camera, GameTime gameTime)
        {
            switch (engine.NetworkType)
            {
                case Network_User_Type.Offline:
                    {
                        if (offlineUser != null && offlineUser.NeedCreate)
                        {
                            offlineUser.NeedCreate = false;
                            CreateOfflinePlayerUnit();
                        }
                    }
                    break;
                case Engine.Multiplayer.Network_User_Type.Server:
                    {
                        var rq = removeQueue.ToArray();
                        #region remove all users in removeQueue so they no longer get updated
                        if (rq.Length > 0)
                        {
                            foreach (var id in rq)
                            {
                                if (!serverUsers.ContainsKey(id))
                                {
                                    Log.Instance.Write($"Could not remove user {id}. Already removed");
                                    continue;
                                }
                                
                                Log.Instance.Write($"{serverUsers[id].Peer.UserName} left the game");
                                MapObject mo = engine.Scene.Map.FindByNetworkUID(serverUsers[id].MapObjectID);

                                serverUsers.Remove(id);

                                if (mo != null)
                                    engine.Scene.Map.QueueObjectRemove(mo.UID);

                                //notify clients of the players removal
                                //their map object will be deleted automatically when removed from the server
                                var msg = ServerCreateMessageHeader(Network_Message.ClientPlayerDisconnected);
                                msg.Write(id);
                                ServerSendMessage(msg);
                            }

                            removeQueue.Clear();
                        }

                        #endregion 

                        #region add all new users waiting

                        if (addQueue.Count > 0)
                        {
                            foreach (var p in addQueue)
                            {
                                serverUsers.Add(p.Peer.Id, p);
                                Log.Instance.Write($"{serverUsers[p.Peer.Id].Peer.UserName} joined the game");

                                //notify clients of the add. the map object will be created automatically when created on the server
                                var msg = ServerCreateMessageHeader(Network_Message.ClientPlayerConnected);
                                msg.Write(p.Peer.Id);
                                ServerSendMessage(msg);
                            }

                            addQueue.Clear();
                        }

                        #endregion

                        foreach (var u in serverUsers.Values)
                            if (u.NeedCreate)
                                CreateClientPlayerUnitOnServer(u);
                    }
                    break;
            }
        }

        private void CreateClientPlayerUnitOnServer(ServerUser u)
        {
            u.NeedCreate = false;
            SpawnPoint sp = engine.Scene.Map.GetFreeRandomSpawnPoint();
            MapObject mo = engine.Scene.CreateMapObject(u.Peer.UserName, _type.PlayerUnit, engine.Scene.Map.GenerateRandomUID(),
                sp.Transform.Translation, sp.Transform.Rotation, sp.Transform.Scale);

            mo.Loaded += (mapObject, gameObject) =>
            {
                //c.SetTransform(sp.Position, sp.Rotation, Vector3.One);
                u.MapObjectID = mo.UID;

                var msg = ServerCreateMessageHeader(Network_Message.ClientLoaded);
                msg.Write(u.Peer.Id);
                msg.Write(mo.UID);
                msg.Write(mapObject.Transform.Translation);
                msg.Write(mapObject.Transform.Rotation);
                ServerSendMessage(msg);
            };

            mo.AddedToMap += (mapObject, gameObject) =>
            {
                //A message has been sent out to create the map object already
                //it is done when AddToScene is called on the map object when the server processes its add/remove queues
                //So this message is specifically for the client this avatar was created for
                //so the client can get an initial update and take control of it
                var msg = ServerCreateMessageHeader(Network_Message.ClientAddedToScene);

                msg.Write(u.Peer.Id);
                msg.Write(mo.UID);
                ServerSendMessage(msg);
            };
        }

        private void CreateOfflinePlayerUnit()
        {
            SpawnPoint sp = engine.Scene.Map.GetFreeRandomSpawnPoint();
            MapObject characterMapObject = engine.Scene.CreateMapObject("Player", _type.PlayerUnit);
            MapObject controllerMapObject = engine.Scene.CreateMapObject("PlayerController", _type.PlayerController);

            characterMapObject.Loaded += (mapObject, gameObject) =>
            {
                AssignIntellectToCharacter(characterMapObject, controllerMapObject);
                gameObject.Transform.Update(sp.Transform.Matrix);

                if (characterMapObject.IsLoaded && controllerMapObject.IsLoaded)
                    AvatarCreated?.Invoke(characterMapObject, controllerMapObject);
            };

            controllerMapObject.Loaded += (mapObject, gameObject) =>
            {
                AssignIntellectToCharacter(characterMapObject, controllerMapObject);

                if (characterMapObject.IsLoaded && controllerMapObject.IsLoaded)
                    AvatarCreated?.Invoke(characterMapObject, controllerMapObject);
            };
        }

        private void AssignIntellectToCharacter(MapObject c, MapObject i)
        {
            if (!c.IsLoaded || !i.IsLoaded)
                return;

            c.GameObject.AssignController((GameObjectController)i.GameObject);
        }

        public void QueuePlayerAdd()
        {
            offlineUser = OfflineUser.Create();
        }

        public void QueuePlayerAdd(ServerConnectedPeer player) => addQueue.Add(ServerUser.Create(player));

        public void QueuePlayerRemove(ServerConnectedPeer player) => removeQueue.Add(player.Id);

        public override void ProcessMessage(ushort uid, ushort mapObjectCommand, NetIncomingMessage message)
        {
            //messages can come through here whether you are a client or server, so we need to filter out what we need

            Network_Message moc = (Network_Message)mapObjectCommand;
            
            switch (engine.NetworkType)
            {
                case Network_User_Type.Server:
                    {
                        Log.Instance.Write("Server received PlayerManager network message: " + moc.ToString());

                        switch (moc)
                        {
                            case Network_Message.RequestPlayerList:
                                {
                                    var msg = ServerCreateMessageHeader(Network_Message.RequestPlayerListResponse);
                                    msg.Write((ushort)serverUsers.Keys.Count);

                                    foreach (var s in serverUsers)
                                    {
                                        msg.Write(s.Key);
                                        msg.Write(s.Value.MapObjectID);
                                        var mo = engine.Scene.Map.FindByNetworkUID(s.Value.MapObjectID);
                                        if (mo == null)
                                        {
                                            Log.Instance.Write($"MapObject {s.Value.MapObjectID} does not exist");
                                            msg.Write(Vector3.Zero);
                                            msg.Write(Quaternion.Identity);
                                            msg.Write(Vector3.One);
                                        }
                                        else
                                        {
                                            Log.Instance.Write($"MapObject {s.Value.MapObjectID} exists");
                                            msg.Write(mo.Transform.Translation);
                                            msg.Write(mo.Transform.Rotation);
                                            msg.Write(mo.Transform.Scale);
                                        }
                                    }
                                    
                                    ServerSendMessage(msg);
                                }
                                break;
                        }
                    }
                    break;
                case Network_User_Type.Client:
                    {
                        Log.Instance.Write("Client received PlayerManager network message: " + moc.ToString());

                        switch (moc)
                        {
                            case Network_Message.ClientAddedToScene:
                                {
                                    short userId = message.ReadInt16();
                                    ushort newMapObjectId = message.ReadUInt16();
                                    clientUsers[userId].MapObjectID = newMapObjectId;
                                }
                                break;
                            case Network_Message.ClientLoaded:
                                {
                                    short userId = message.ReadInt16();
                                    ushort newMapObjectId = message.ReadUInt16();
                                    Vector3 pos = message.ReadVector3();
                                    Quaternion rot = message.ReadQuaternion();

                                    MapObject mo = engine.Scene.Map.FindByNetworkUID(newMapObjectId);
                                    if (mo != null)
                                        mo.GameObject.Transform.Update(rot, pos);
                                }
                                break;
                            case Network_Message.ClientPlayerConnected:
                                {
                                    short userId = message.ReadInt16();

                                    if (userId == engine.NetworkClient.ThisUser.Id)
                                    {
                                        //we need to request a list of the other players on the server
                                        var msg = ClientCreateMessageHeader(Network_Message.RequestPlayerList);
                                        msg.Write(userId);
                                        ClientSendMessage(msg);
                                    }

                                    ClientConnectedPeer peer = engine.NetworkClient.Peers[userId];
                                    clientUsers.Add(userId, ClientUser.Create(peer));
                                }
                                break;
                            case Network_Message.ClientPlayerDisconnected:
                                {
                                    short userId = message.ReadInt16();
                                    clientUsers.Remove(userId);
                                }
                                break;
                            case Network_Message.RequestPlayerListResponse:
                                {
                                    ushort length = message.ReadUInt16();
                                    for (ushort i = 0; i < length; i++)
                                    {
                                        short key = message.ReadInt16();
                                        ushort mapObjectID = message.ReadUInt16();
                                        Vector3 pos = message.ReadVector3();
                                        Quaternion rot = message.ReadQuaternion();
                                        Vector3 scl = message.ReadVector3();

                                        if (clientUsers.ContainsKey(key))
                                            continue;

                                        ClientConnectedPeer peer = engine.NetworkClient.Peers[key];
                                        clientUsers.Add(key, ClientUser.Create(peer));

                                        MapObject mo = engine.Scene.CreateMapObject(peer.UserName, _type.PlayerUnit, mapObjectID, pos, rot, scl);
                                        engine.Scene.Map.QueueObjectAdd(mo);
                                    }
                                }
                                break;
                            default:
                                Debugger.Break();
                                break;
                        }
                    }
                    break;
            }
        }

        public NetOutgoingMessage ClientCreateMessageHeader(Network_Message messageType)
        {
            var msg = engine.NetworkClient.NetworkObject.CreateMessage();
            msg.Write((ushort)Custom_Message_Type.MapObjectInteraction);
            msg.Write((ushort)messageType);
            //send the uid for this map object so the client knows which map object is talking to it
            msg.Write(uid);

            return msg;
        }

        public void ClientSendMessage(NetOutgoingMessage msg)
        {
            engine.NetworkClient.NetworkObject.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public NetOutgoingMessage ServerCreateMessageHeader(Network_Message messageType)
        {
            var msg = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.MapObjectInteraction);
            msg.Write((ushort)messageType);
            //send the uid for this map object so the client knows which map object is talking to it
            msg.Write(uid);

            return msg;
        }

        public void ServerSendMessage(NetOutgoingMessage msg)
        {
            //todo: we should have a parameter to remove certain serverUSers from receiving the message
            if (serverUsers.Count == 0)
                return;

            List<NetConnection> serverUserConnections = new List<NetConnection>();

            foreach (var user in serverUsers.Values)
                serverUserConnections.Add(user.Peer.Connection);

            engine.NetworkServer.NetworkObject.SendMessage(msg, serverUserConnections, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }
}
