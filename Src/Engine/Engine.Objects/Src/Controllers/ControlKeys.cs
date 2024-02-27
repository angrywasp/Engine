namespace Engine.Objects.Controllers
{
    public static class ControlKeys
    {
        public const int None = 0;

        public const int Forward = 1;
        public const int Backward = 2;
        public const int Left = 3;
        public const int Right = 4;
        public const int LookUp = 5;
        public const int LookDown = 6;
        public const int LookLeft = 7;
        public const int LookRight = 8;
        public const int Run = 9;
        public const int Fire = 10;


        public const int Jump = 11;
        public const int Crouch = 12;
        public const int AltFire = 13;
        public const int Reload = 14;
        public const int Use = 15;
        public const int PreviousWeapon = 16;
        public const int NextWeapon = 17;
        public const int Weapon1 = 18;
        public const int Weapon2 = 19;
        public const int Weapon3 = 20;
        public const int Weapon4 = 21;
        public const int ChangeCamera = 22;

        public const int KeyCount = 23;
    }

    public enum Camera_Type
    {
        Fps,
        Tps,
        Invalid
    }
}
