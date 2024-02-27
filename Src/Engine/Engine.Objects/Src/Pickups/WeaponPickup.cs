using Engine.World;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects.Pickups
{
    public class WeaponPickupType : PickupType
    {
        [JsonProperty] public string WeaponType { get; set; }
    }

    public class WeaponPickup : Pickup
    {
        private WeaponPickupType _type = null;

        public new WeaponPickupType Type => _type;

        public override void Take(Unit taker)
        {
            base.Take(taker);
            taker.SetWeapon(_type.WeaponType, null);
        }
    }
}