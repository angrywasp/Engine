using Engine.Multiplayer;

namespace Engine.World.Objects.Users
{
    public class ServerUser : UserBase
    {
        public ServerConnectedPeer Peer { get; set; }

        public static ServerUser Create(ServerConnectedPeer peer)
        {
            ServerUser p = new ServerUser();
            p.Peer = peer;
            p.NeedCreate = true; //just been created so we need to have an avatar
            return p;
        }
    }
}
