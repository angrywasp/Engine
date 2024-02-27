using Engine.Interfaces;
using Lidgren.Network;
using System.Collections.Generic;
using System.Diagnostics;
using AngryWasp.Logger;
using System.Text;
using AngryWasp.Random;
using Engine.World;

namespace Engine.Multiplayer
{
    public class NetworkClient : INetworkPeer<NetClient>
    {
        public delegate void NetworkClientChatEventHandler(ClientConnectedPeer sender, string message);
        public delegate void NetworkClientEventHandler(ClientConnectedPeer sender);
        public delegate void NetworkClientServerMapLoadEventHandler(bool isServerMapLoaded, string mapFile);
        public event NetworkClientChatEventHandler IncomingPrivateChat;
        public event NetworkClientChatEventHandler IncomingPublicChat;
        public event NetworkClientEventHandler Connected;
        public event NetworkClientEventHandler Disconnected;
        public event NetworkClientServerMapLoadEventHandler MapFile;

        NetPeerConfiguration config;
        NetClient client;
        EngineCore engine;

        private Dictionary<short, ClientConnectedPeer> peers = new Dictionary<short, ClientConnectedPeer>();
        private ClientConnectedPeer thisUser;

        public Dictionary<short, ClientConnectedPeer> Peers => peers;

        public ClientConnectedPeer ThisUser => thisUser;

        public bool IsConnected
        {
            get
            {
                if (client == null)
                    return false;

                if (client.ConnectionStatus != NetConnectionStatus.Connected)
                    return false;

                return true;
            }
        }

        public NetClient NetworkObject => client;

        public NetworkClient(EngineCore engine, string address, int port, string privateKey)
        {
            this.engine = engine;
            config = new NetPeerConfiguration(privateKey); // needs to be same on client and server!
            client = new NetClient(config);
        }

        public void Update()
        {
            NetIncomingMessage message;
            while ((message = client.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            Custom_Message_Type cmt = (Custom_Message_Type)message.ReadInt16();

                            //Log.Instance.Write("Received Message: " + cmt.ToString());

                            switch (cmt)
                            {
                                case Custom_Message_Type.PublicChat:
                                    {
                                        ClientConnectedPeer sender = peers[message.ReadInt16()];
                                        string content = message.ReadString();

                                        IncomingPublicChat?.Invoke(sender, content);
                                    }
                                    break;
                                case Custom_Message_Type.PrivateChat:
                                    {
                                        ClientConnectedPeer sender = peers[message.ReadInt16()];
                                        string content = message.ReadString();

                                        IncomingPrivateChat?.Invoke(sender, content);
                                    }
                                    break;

                                case Custom_Message_Type.ThisUserConnected:
                                    {
                                        short senderId = message.ReadInt16();
                                        short count = message.ReadInt16();

                                        for (int i = 0; i < count; i++)
                                        {
                                            short id = message.ReadInt16();
                                            string username = message.ReadString();

                                            if (peers.ContainsKey(id))
                                                continue;

                                            peers.Add(id, new ClientConnectedPeer()
                                                {
                                                    Id = id,
                                                    UserName = username
                                                });
                                        }

                                        thisUser = peers[senderId];
                                        Connected?.Invoke(thisUser);
                                    }
                                    break;
                                case Custom_Message_Type.UserConnected:
                                    {
                                        short id = message.ReadInt16();
                                        string username = message.ReadString();

                                        var x = new ClientConnectedPeer()
                                            {
                                                Id = id,
                                                UserName = username
                                            };

                                        peers.Add(id, x);

                                        Connected?.Invoke(x);

                                        Log.Instance.Write($"User {username} connected");
                                    }
                                    break;
                                case Custom_Message_Type.UserDisconnected:
                                    {
                                        ClientConnectedPeer sender = peers[message.ReadInt16()];
                                        string msg = message.ReadString();

                                        peers.Remove(sender.Id);

                                        Log.Instance.Write($"User {sender.UserName} disconnected. {msg}");
                                    }
                                    break;
                                case Custom_Message_Type.MapLoaded:
                                    {
                                        bool isServerMapLoaded = message.ReadBoolean();
                                        MapFile?.Invoke(isServerMapLoaded, message.ReadString());
                                    }
                                    break;
                                case Custom_Message_Type.MapInitialUpdateResponse:
                                    {
                                        //todo: update client map from server information

                                        //until now we have connected to a server and identified ourselves
                                        //asked the server what map it is running and loaded it
                                        //then we asked the server to send us a full world update
                                        //which is stored in this message.
                                        //now we send back to the server that we are ready to join the game (finally)
                                        //this will add us to the list of players on the server that will receive 
                                        //game world updates

                                        InformServerClientIsReady();
                                    }
                                    break;

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

                                #region Map

                                case Custom_Message_Type.Map:
                                    {
                                        ushort mapCommand = message.ReadUInt16();
                                        engine.Scene.Map.ProcessMessage(mapCommand, message);
                                    }
                                    break;

                                #endregion

                                default:
                                    {
                                        Log.Instance.WriteWarning("Unhandled custom message with type: " + cmt.ToString());
                                    }
                                    break;
                            }
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        {
                            switch (message.SenderConnection.Status)
                            {
                                case NetConnectionStatus.Connected:
                                    {
                                        //the server has accepted our connection
                                        //now we tell everybody who we are
                                        //todo: for now we generate a random string for our name
                                        //but this could be just a text name (if a local lan server)
                                        //or from a user database
                                        //todo: provide a real user name
                                        UpdatePlayerDetails(RandomString.AlphaNumeric(4));
                                    }
                                    break;
                            }

                            Log.Instance.Write("Connection status: " + message.SenderConnection.Status);
                        }
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        Log.Instance.Write(message.ReadString());
                        break;
                    default:
                        Log.Instance.WriteWarning($"Unhandled message with type: {message.MessageType}, Data: {Encoding.ASCII.GetString(message.Data)}");
                        break;
                }
            }
        }

        private NetOutgoingMessage CreateMessageHeader(Custom_Message_Type messageType)
        {
            if (thisUser == null)
                Debugger.Break();

            var msg = client.CreateMessage();
            msg.Write((short)messageType);
            msg.Write(thisUser.Id);

            return msg;
        }

        //Sends a public chat message to the server
        public void PublicChat(string message)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.PublicChat);
            msg.Write(message);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        //sends a private chat message to the server to be redirected to another user
        public void PrivateChat(short recipientId, string message)
        {
            var msg = CreateMessageHeader(Custom_Message_Type.PrivateChat);
            msg.Write(recipientId);
            msg.Write(message);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        //sends our information (username etc) to the server
        public void UpdatePlayerDetails(string username)
        {
            var msg = client.CreateMessage();
            msg.Write((ushort)Custom_Message_Type.UpdatePlayerDetails);
            msg.Write(username);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void RequestMapFile()
        {
            var msg = CreateMessageHeader(Custom_Message_Type.MapFileRequest);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public void RequestInitialMapUpdate()
        {
            var msg = CreateMessageHeader(Custom_Message_Type.MapInitialUpdateRequest);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private void InformServerClientIsReady()
        {
            var msg = CreateMessageHeader(Custom_Message_Type.ClientIsReady);
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
