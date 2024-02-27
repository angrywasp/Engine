using Engine.World.Objects;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class WeaponType : DynamicType
    {
        [JsonProperty] public string PrimaryBulletType { get; set; }
        [JsonProperty] public string SecondaryBulletType { get; set; }
        [JsonProperty] public string BoneName { get; set; } //RightHand
    }

    public class Weapon : Dynamic
    {
        #region Required in every script class

        private WeaponType _type = null;

        public new WeaponType Type => _type;

        #endregion

        public void PrimaryFire()
        {
            //todo: need a muzzle offset
            engine.Scene.CreateMapObject<RuntimeMapObject>(null, _type.PrimaryBulletType, this.Transform.Rotation, this.Transform.Translation);
        }
    }
}