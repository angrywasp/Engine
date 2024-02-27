using Lidgren.Network;

namespace Engine.Interfaces
{
    interface INetworkPeer<T> where T : NetPeer
    {
        T NetworkObject { get; }

        void Update();
    }
}
