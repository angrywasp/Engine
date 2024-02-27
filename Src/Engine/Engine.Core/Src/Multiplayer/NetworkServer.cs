using Engine.Interfaces;
using Lidgren.Network;
using System.Collections.Generic;
using System.Net;
using AngryWasp.Logger;
using Engine.Helpers;
using System.Numerics;
using Engine.World;

namespace Engine.Multiplayer
{
    public class NetworkServer : INetworkPeer<NetServer>
    {
        public delegate void NetworkServerChatEventHandler(ServerConnectedPeer sender, string message);
        public delegate void NetworkServerEventHandler(ServerConnectedPeer sender);
        public event NetworkServerChatEventHandler IncomingPrivateChat;
        public event NetworkServerChatEventHandler IncomingPublicChat;
        public event NetworkServerEventHandler Connected;
        public event NetworkServerEventHandler Disconnected;

        NetPeerConfiguration config;
        NetServer server;
        EngineCore engine;

        string mapFile;
        bool mapLoaded;

        //todo: need to disconnect connected users if they have not identified themselves in an appropriate amount of time
        private Dictionary<NetConnection, float> connectedPeers = new Dictionary<NetConnection, float>();
        private Dictionary<short, ServerConnectedPeer> identifiedPeers = new Dictionary<short, ServerConnectedPeer>();
        private Dictionary<short, NetConnection> identifiedPeerConnections = new Dictionary<short, NetConnection>();

        public NetServer NetworkObject
        {
            get { return server; }
        }

        public NetworkServer(EngineCore engine, string address, int port, string privateKey, int maxConnections, string mapFile)
        {
            this.engine = engine;
            config = new NetPeerConfiguration(privateKey); // needs to be same on client and server!
            config.MaximumConnections = maxConnections;
            config.LocalAddress = IPAddress.Parse(address);
            config.Port = port;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            this.mapFile = mapFile;
            server = new NetServer(config);

            engine.Scene.MapLoaded += (path, map) =>
            {
                mapLoaded = mapFile == path;
            };
        }

        public void Update()
        {
            NetIncomingMessage message;
            while ((message = server.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            Custom_Message_Type cmt = (Custom_Message_Type)message.ReadInt16();

                            //Log.Instance.Write("Received Message: " + cmt.ToString());
 
                            switch (cmt)
                            {
                                #region PrivateChat

                                case Custom_Message_Type.PrivateChat:
                                    {
                                        short senderId = message.ReadInt16();
                                        short recipientId = message.ReadInt16();
                                        string content = message.ReadString();

                                        ServerConnectedPeer sender = identifiedPeers[senderId];
                                        ServerConnectedPeer recipient = identifiedPeers[recipientId];

                                        Log.Instance.Write($"{sender.UserName} -> {recipient.UserName}: {content}");

                                        IncomingPrivateChat?.Invoke(sender, content);

                                        PrivateChat(sender.Id, recipient.Connection, content);
                                    }
                                    break;

                                #endregion

                                #region PublicChat

                                case Custom_Message_Type.PublicChat:
                                    {
                                        short senderId = message.ReadInt16();
                                        string content = message.ReadString();

                                        ServerConnectedPeer sender = identifiedPeers[senderId];

                                        Log.Instance.Write($"{sender.UserName} -> All: {content}");

                                        IncomingPublicChat?.Invoke(sender, content);

                                        PublicChat(senderId, content);
                                    }
                                    break;

                                #endregion

                                #region UpdatePlayerDetails

                                case Custom_Message_Type.UpdatePlayerDetails:
                                    {
                                        string newName = message.ReadString();
                                        short newId = 0;

                                        while (identifiedPeers.ContainsKey(newId))
                                            ++newId;

                                        ServerConnectedPeer cp = new ServerConnectedPeer()
                                        {
                                            UserName = newName,
                                            Connection = message.SenderConnection,
                                            Id = newId
                                        };

                                        message.SenderConnection.Tag = newId;
                                        identifiedPeers.Add(newId, cp);
                                        identifiedPeerConnections.Add(newId, message.SenderConnection);
                                        connectedPeers.Remove(message.SenderConnection);

                                        Log.Instance.Write($"{message.SenderEndPoint.Address} identified as {newName}");

                                        Connected?.Invoke(cp);

                                        //send a message to all clients that the user has entered the server
                                        UserConnected(newId, newName);
                                        ThisUserConnected(newId, message.SenderConnection);
                                    }
                                    break;

                                #endregion

                                #region MapFileRequest

                                case Custom_Message_Type.MapFileRequest:
                                    {
                                        short senderId = message.ReadInt16();
                                        MapFileRequestResponse(identifiedPeers[senderId].Connection, mapFile);
                                    }
                                    break;

                                #endregion

                                #region MapUpdateRequest

                                case Custom_Message_Type.MapInitialUpdateRequest:
                                    {
                                        short senderId = message.ReadInt16();
                                        MapUpdateRequestResponse(identifiedPeers[senderId].Connection);
                                    }
                                    break;

                                #endregion

                                #region MapObjectInteraction

                                case Custom_Message_Type.MapObjectInteraction:
                                    {
                                        ushort mapObjectCommand = message.ReadUInt16();
                                        ushort mapObjectId = message.ReadUInt16();

                                        MapObject mo = engine.Scene.Map.FindByNetworkUID(mapObjectId);
                                        mo.GameObject.ProcessMessage(mapObjectId, mapObjectCommand, message);
                                    }
                                    break;

                                #endregion

                                #region MapObjectUpdate

                                case Custom_Message_Type.MapObjectUpdate:
                                    {
                                        ushort uid = message.ReadUInt16();
                                        ushort cmd = message.ReadUInt16();

                                        MapObject mo = engine.Scene.Map.FindByNetworkUID(uid);
                                        if (mo != null)
                                            mo.GameObject.ProcessMessage(uid, cmd, message);
                                    }
                                    break;

                                #endregion

                                #region ClientIsReady

                                case Custom_Message_Type.ClientIsReady:
                                    {
                                        short senderId = message.ReadInt16();
                                        //client has indicated they are loaded up and ready to play
                                        var sender = identifiedPeers[senderId];
                                        engine.Scene.Map.ServerAddClientPlayerToMap(sender);
                                    }
                                    break;

                                #endregion

                                #region Not implemented

                                default:
                                    {
                                        Log.Instance.WriteWarning("Unhandled custom message with type: " + cmt.ToString());
                                    }
                                    break;

                                #endregion
                            }
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        {
                            switch (message.SenderConnection.Status)
                            {
                                case NetConnectionStatus.Disconnected:
                                    {
                                        short id = (short)message.SenderConnection.Tag;
                                        string msg = message.ReadString();

                                        var user = identifiedPeers[id];
                                        
                                        identifiedPeers.Remove(id);
                                        identifiedPeerConnections.Remove(id);
                                        UserDisconnected(id, msg);
                                        Log.Instance.Write($"User {user.UserName} disconnected");

                                        Disconnected?.Invoke(user);
                                    }
                                    break;
                                default:
                                    {
                                        Log.Instance.Write("Incoming connection status: " + message.SenderConnection.Status.ToString());
                                    }
                                    break;
                            }
                        }
                        break;

                    case NetIncomingMessageType.DebugMessage:
                        Log.Instance.Write(message.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Log.Instance.WriteWarning(message.ReadString());
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            message.SenderConnection.Approve();
                            connectedPeers.Add(message.SenderConnection, 0);
                        }
                        break;
                    default:
                        Log.Instance.WriteWarning("Unhandled message with type: " + message.MessageType);
                        break;
                }
            }
        }

        public NetOutgoingMessage CreateMessageHeader(Custom_Message_Type messageType)
        {
            var msg = server.CreateMessage();
            msg.Write((short)messageType);

            return msg;
        }

        public void PublicChat(short senderId, string message)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.PublicChat);
            msg.Write(senderId);
            msg.Write(message);

            SendToAllExcept(senderId, msg);
        }

        private void SendToAll(NetOutgoingMessage msg)
        {
            if (identifiedPeerConnections.Count == 0)
                return;

            List<NetConnection> peerConnectionList = new List<NetConnection>(identifiedPeerConnections.Values);
            server.SendMessage(msg, peerConnectionList, NetDeliveryMethod.ReliableOrdered, 0);
        }

        private void SendToAllExcept(short senderId, NetOutgoingMessage msg)
        {
            if (identifiedPeerConnections.Count - 1 <= 0)
                return;

            List<NetConnection> peerConnectionList = new List<NetConnection>(identifiedPeerConnections.Values);

            if (identifiedPeers.ContainsKey(senderId))
                peerConnectionList.Remove(identifiedPeers[senderId].Connection);

            server.SendMessage(msg, peerConnectionList, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void PrivateChat(short senderId, NetConnection recipient, string message)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.PrivateChat);
            msg.Write(senderId);
            msg.Write(message);

            server.SendMessage(msg, recipient, NetDeliveryMethod.ReliableOrdered);
        }

        public void UserConnected(short id, string username)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.UserConnected);
            msg.Write(id);
            msg.Write(username);

            SendToAllExcept(id, msg);
        }

        public void ThisUserConnected(short id, NetConnection recipient)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.ThisUserConnected);
            msg.Write(id);

            msg.Write((short)identifiedPeers.Count);

            foreach (var ip in identifiedPeers)
            {
                msg.Write(ip.Key);
                msg.Write(ip.Value.UserName);
            }

            server.SendMessage(msg, recipient, NetDeliveryMethod.ReliableOrdered);
        }

        public void UserDisconnected(short id, string goodbyeMessage)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.UserDisconnected);
            msg.Write(id);
            msg.Write(goodbyeMessage);

            SendToAllExcept(id, msg);
        }

        public void MapLoaded()
        {
            mapLoaded = true;
            var msg = CreateMessageHeader(Custom_Message_Type.MapLoaded);
            msg.Write(mapLoaded);
            msg.Write(mapFile);

            SendToAll(msg);
        }

        public void MapFileRequestResponse(NetConnection recipient, string mapPath)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.MapLoaded);            
            msg.Write(mapLoaded);
            msg.Write(mapPath);

            server.SendMessage(msg, recipient, NetDeliveryMethod.ReliableOrdered);
        }

        private void MapUpdateRequestResponse(NetConnection recipient)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.MapInitialUpdateResponse);

            //this message will be generated when a client has loaded their map
            //and the server map is loaded. 

            //todo: serialize the world and send it to the client 

            server.SendMessage(msg, recipient, NetDeliveryMethod.ReliableOrdered);
        }

        /*internal void UpdatePhysicsWorld()
        {
            var msg = CreateMessageHeader(Custom_Message_Type.PhysicsUpdate);

            Space sp = engine.Scene.Physics;

            int count = 0;

            foreach (var e in sp.Entities)
            {
                if (!e.IsDynamic)
                    continue;

                ++count;
            }

            msg.Write(count);

            foreach (var e in sp.Entities)
            {
                if (!e.IsDynamic)
                    continue;

                Vector3 p = e.Position;
                Quaternion q = e.Orientation;
                Vector3 lv = e.LinearVelocity;
                Vector3 av = e.AngularVelocity;
                msg.Write(e.InstanceId);
                msg.Write(p);
                msg.Write(q);
                msg.Write(lv);
                msg.Write(av);
            }

            server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
        }*/
    }
}
