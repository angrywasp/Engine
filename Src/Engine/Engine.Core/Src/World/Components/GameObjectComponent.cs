using System.Numerics;
using System.Threading.Tasks;
using Engine.Cameras;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine.World.Components
{
    public class GameObjectComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.GameObjectComponent";
        
        [JsonProperty] public string GameObjectPath { get; set; }
    }

    public class GameObjectComponent : Component
    {
        private GameObjectComponentType _type = null;

        public new GameObjectComponentType Type => _type;

        protected GameObject gameObject;

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            gameObject = await parent.engine.TypeFactory.CreateGameObjectAsync<GameObject>(_type.GameObjectPath).ConfigureAwait(false);
            gameObject.OnAddedToMap();
            await base.CreateFromTypeAsync(parent).ConfigureAwait(false);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);
            gameObject.Transform.Update(this.GlobalTransform.Matrix);
            gameObject.Update(camera, gameTime);
        }

        /*public override void UpdateTransforms(Matrix4x4 localTransform)
        {
            base.UpdateTransforms(localTransform);

            Vector3 scl;
            Quaternion rot;
            Vector3 pos;

            Matrix4x4.Decompose(globalTransform, out scl, out rot, out pos);

            gameObject.SetTransform(pos, rot, scl);
            gameObject.UpdatePhysicsTransform();
        }*/
    }
}