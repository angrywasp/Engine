using Engine.Multiplayer;

namespace Engine.World.Objects.Users
{
    public class ClientUser : UserBase
    {
        public ClientConnectedPeer Peer { get; set; }

        public static ClientUser Create(ClientConnectedPeer peer)
        {
            var p = new ClientUser();
            p.UserId = peer.Id;
            p.Peer = peer;
            //todo: i forget why this is false. find out and update comments
            p.NeedCreate = false;
            //p.Controller = Core.Scene.CreateMapObject(peer.UserName + "Controller", PlayerManager.Instance.Type.PlayerController, Core.Scene.Map.GenerateRandomNetworkID());
            //p.Controller.Load(Core.Scene);
            return p;
        }
    }
}
