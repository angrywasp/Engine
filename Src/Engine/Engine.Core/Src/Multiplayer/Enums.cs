using System.Diagnostics;
using System.Net;

namespace Engine.Multiplayer
{
    public enum Network_User_Type
    {
        Offline,
        Server,
        Client
    }

    public enum Custom_Message_Type : ushort
    {
        PublicChat,
        PrivateChat,
        UserConnected,
        ThisUserConnected,
        UserDisconnected,
        UpdatePlayerDetails,
        MapLoaded,
        
        MapObjectUpdate,
        PhysicsUpdate,
        MapFileRequest,
        MapInitialUpdateRequest,
        MapInitialUpdateResponse,
        MapObjectInteraction,
        ClientIsReady,
        Map, //the map has something to say
        //Info,
    }
}
