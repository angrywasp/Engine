using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class GunType : WeaponType
    {
        [JsonProperty] public string BulletType { get; set; }
    }

    public class Gun : Weapon
    {
        #region Required in every script class

        private GunType _type = null;

        public new GunType Type => _type;

        #endregion
    }
}