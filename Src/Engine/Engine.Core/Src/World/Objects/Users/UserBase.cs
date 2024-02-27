namespace Engine.World.Objects.Users
{
    public class UserBase
    {
        //public MapObject Controller { get; set; }

        public MapObject Avatar { get; set; }

        public short UserId { get; set; }

        public ushort MapObjectID { get; set; }

        //indicate if this user needs an in game representation
        //on the client is represents that we need to ask the server for a new avatar (got killed etc)
        //on the server it represents if we need to create a new avatar for a client
        public bool NeedCreate { get; set; }
    }
}
