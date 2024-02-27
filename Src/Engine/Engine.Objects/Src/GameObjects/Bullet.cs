using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine.Cameras;
using Engine.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace Engine.Objects.GameObjects
{
    public class BulletType : DynamicType
    {
        [JsonProperty] public float Velocity { get; set; } = 10;

        [JsonProperty] public float MaxDistance { get; set; } = 100;

        [JsonProperty] public float Damage { get; set; } = 5;

        [JsonProperty] public string FireSound { get; set; }
    }

    public class Bullet : Dynamic
    {
        #region Required in every script class

        private BulletType _type = null;

        public new BulletType Type => _type;

        #endregion

        private Vector3 origin;
        private Vector3 startPosition;
        private Vector3 velocity;

        private SoundEffect fireSound;

        public override async Task LoadAsync(EngineCore engine)
        {
            fireSound = await ContentLoader.LoadSoundEffectAsync(_type.FireSound).ConfigureAwait(false);
            await base.LoadAsync(engine).ConfigureAwait(false);
        }

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            fireSound.CreateInstance().Play();
            startPosition = origin = transform.Translation;
            velocity = transform.Matrix.Forward() * _type.Velocity;
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            if (IsDead)
                return;

            var distanceTravelled = (transform.Translation - origin).Length();
            if (distanceTravelled >= _type.MaxDistance)
            {
                Die();
                return;
            }

            Vector3 offset = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float dist = offset.Length();

            startPosition = transform.Translation;

            var hits = engine.Scene.Physics.RayCast(new Ray(startPosition, offset, dist));

            if (hits.Count > 0)
            {
                //hit something
                Log.Instance.Write("Bullet hit");
                Die();
            }
        }
    }
}