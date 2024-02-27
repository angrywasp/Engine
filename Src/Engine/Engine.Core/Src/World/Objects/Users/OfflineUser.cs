namespace Engine.World.Objects.Users
{
    public class OfflineUser : UserBase
    {
        public static OfflineUser Create()
        {
            OfflineUser p = new OfflineUser();
            p.NeedCreate = true;
            return p;
        }
    }
}
