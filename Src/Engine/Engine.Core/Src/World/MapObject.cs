using Microsoft.Xna.Framework;
using Engine.Cameras;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using AngryWasp.Logger;
using System.Diagnostics;

namespace Engine.World
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MapObject
    {
        public event EngineEventHandler<MapObject, GameObject> Loaded;
        public event EngineEventHandler<MapObject, GameObject> AddedToMap;
        public event EngineEventHandler<MapObject, GameObject> RemovedFromMap;

        [JsonProperty] public string GameObjectPath { get; set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public WorldTransform3 InitialTransform { get; set; } = new WorldTransform3();

        protected internal EngineCore engine;
        protected internal WorldTransform3 transform = new WorldTransform3();
        protected internal GameObject gameObject = null;
        protected internal ushort uid = 0;
        private bool isLoaded;
        private bool firstLoad = true;

        public WorldTransform3 Transform => transform;
        public GameObject GameObject => gameObject;
        public ushort UID => uid;
        public bool IsLoaded => isLoaded;

        public MapObject() { }

        public MapObject(string gameObjectPath, string name, ushort uid)
        {
            this.GameObjectPath = gameObjectPath;
            this.Name = name;
            this.uid = uid;
        }

        public virtual async Task LoadAsync(EngineCore engine)
        {
            this.engine = engine;
            transform.TransformChanged += (t) =>
            {
                gameObject.Transform.Update(t.Matrix);
                if (gameObject.PhysicsModel != null)
                    gameObject.PhysicsModel.EditorUpdate(t.Matrix);
            };

            Log.Instance.Write($"Loading map object {this.uid}");
            await LoadGameObjectAsync().ConfigureAwait(false);
            Log.Instance.Write($"Loaded map object {this.uid}");

            //need to load the game object before updating the transform
            transform.Update(InitialTransform.Matrix);

            isLoaded = true;
            Loaded?.Invoke(this, gameObject);
        }

        public void OnAddedToMap()
        {
            gameObject.OnAddedToMap();
            AddedToMap?.Invoke(this, gameObject);
        }

        public void OnRemovedFromMap()
        {
            gameObject.OnRemovedFromMap();
            RemovedFromMap?.Invoke(this, gameObject);
        }

        internal void Update(Camera camera, GameTime gameTime) => gameObject.Update(camera, gameTime);

        protected virtual async Task LoadGameObjectAsync()
        {
            try
            {
                if (firstLoad && gameObject != null)
                {
                    firstLoad = false;
                    TypeFactoryItem i = engine.TypeFactory.Items[GameObjectPath];
                    engine.TypeFactory.SetTypeFields(i.ClassType, gameObject, i.TypeInstance);
                }
                else
                {
                    if (GameObjectPath == null)
                    {
                        gameObject = new GameObject();
                        engine.TypeFactory.SetTypeFields(typeof(GameObject), gameObject, new GameObjectType());
                        await gameObject.LoadAsync(engine).ConfigureAwait(false);
                    }
                    else
                        gameObject = await engine.TypeFactory.CreateGameObjectAsync<GameObject>(GameObjectPath).ConfigureAwait(false);
                }

                if (gameObject != null)
                {
                    gameObject.uid = uid;
                    gameObject.name = Name;
                }
                else
                    Log.Instance.WriteFatalException(new Exception("GameObject is null"));
            }
            catch (Exception ex)
            {
                Log.Instance.WriteFatalException(ex);
            }
        }

        public override string ToString() => GameObjectPath?.ToString();
    }
}
