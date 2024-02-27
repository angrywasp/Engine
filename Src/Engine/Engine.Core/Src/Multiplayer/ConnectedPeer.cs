using Lidgren.Network;

namespace Engine.Multiplayer
{
    public class ClientConnectedPeer
    {
        public short Id { get; set; }

        public string UserName { get; set; }
    }

    public class ServerConnectedPeer : ClientConnectedPeer
    {
        public NetConnection Connection { get; set; }
    }
}
