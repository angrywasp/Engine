using System.Threading.Tasks;
using Engine.Content;
using Engine.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects.Pickups
{
    public class PickupType : DynamicType
    {
        [JsonProperty] public string PickupSound { get; set; }
    }

    public class Pickup : Dynamic
    {
        private PickupType _type = null;

        public new PickupType Type => _type;

        private SoundEffect pickupSound;

        public override async Task LoadAsync(EngineCore engine)
        {
            pickupSound = await ContentLoader.LoadSoundEffectAsync(_type.PickupSound).ConfigureAwait(false);
            await base.LoadAsync(engine).ConfigureAwait(false);
        }

        public virtual void Take(Unit taker)
        {
            pickupSound.CreateInstance().Play();
            Die();
            engine.Interface.ScreenMessages.Write($"{taker.Name} picked up item", Color.White);
        }
    }
}