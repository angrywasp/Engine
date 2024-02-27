using Engine.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Engine.Cameras;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Engine.World.Components
{
    public class SoundComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.SoundComponent";

        [JsonProperty] public string Sound { get; set; }

        [JsonProperty] public float Volume { get; set; } = 1.0f;

        [JsonProperty] public bool Loop { get; set; }
    }

    /// <summary>
    /// provides the ability to attach a sound to an entity
    /// </summary>
    public class SoundComponent : Component
    {
        private SoundComponentType _type = null;

        public new SoundComponentType Type => _type;

        private Sound sound;
		private SoundEffect soundEffect;
        private SoundEffectInstance soundInstance;
        private AudioListener l = new AudioListener();
        private AudioEmitter e = new AudioEmitter();

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            
            if (sound == null)
                return;

            l.Forward = camera.Transform.Forward();
            l.Up = camera.Transform.Up();
            l.Position = camera.Transform.Translation;

            soundInstance.Apply3D(l, e);
        }

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            soundEffect = await ContentLoader.LoadSoundEffectAsync(_type.Sound).ConfigureAwait(false);
            soundInstance = soundEffect.CreateInstance();
            soundInstance.Volume = _type.Volume;
            soundInstance.IsLooped = _type.Loop;

            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
        }

        protected override void OnGlobalTransformChanged(WorldTransform3 globalTransform)
        {
            base.OnGlobalTransformChanged(globalTransform);
            e.Forward = globalTransform.Matrix.Forward();
            e.Up = globalTransform.Matrix.Up();
            e.Position = globalTransform.Translation;
        }
    }
}