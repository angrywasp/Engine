using System.Numerics;
using Engine.Content.Model.Instance;
using Engine.World.Components;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class UnitType : DynamicType
    {
        [JsonProperty] public bool AllowPlayerControl { get; set; }

        [JsonProperty] public Vector3 FpsCameraOffset { get; set; }

        [JsonProperty] public Vector3 TpsCameraOffset { get; set; }

        [JsonProperty] public float ViewRadius { get; set; } = 10.0f;
    }

    public class Unit : Dynamic
    {
        #region Required in every script class

        private UnitType _type = null;

        public new UnitType Type => _type;

        #endregion

        private Weapon weapon;
        private SkeletonNodeInstance bone;

        public void SetWeapon(string weaponType, string boneName)
        {
#pragma warning disable CS4014
            this.LoadComponentAsync(new GameObjectComponentType
            {
                Name = "Gun",
                GameObjectPath = weaponType,
                LocalTransform = WorldTransform3.Create(Vector3.UnitX)
            });
#pragma warning restore CS4014 
        }

        public bool HasWeapon()
        {
            return weapon != null;
        }
    }
}
