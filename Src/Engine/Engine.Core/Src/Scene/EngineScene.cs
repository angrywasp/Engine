using Engine.Content;
using Engine.Helpers;
using Engine.Multiplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.Cameras;
using System.Numerics;
using AngryWasp.Logger;
using Engine.World;
using AngryWasp.Random;
using System.Threading.Tasks;

namespace Engine.Scene
{
    //this contains the full world for a game
    public class EngineScene
    {
        private delegate void PhysicsUpdateDelegate(GameTime gameTime);

        public event EngineEventHandler<string, Map> MapLoaded;

        private EngineCore engine;
        private GraphicsDevice graphicsDevice;

        private SceneGraphics graphics;
        private ScenePhysics physics;
        private PhysicsUpdateDelegate physicsUpdate;

        private Map map;

        public Map Map => map;

        public SceneGraphics Graphics => graphics;

        public ScenePhysics Physics => physics;

        public EngineScene(EngineCore engine)
        {
            this.engine = engine;
            graphicsDevice = engine.GraphicsDevice;

            graphics = new SceneGraphics(engine);
            physics = new ScenePhysics(engine);
            physicsUpdate = new PhysicsUpdateDelegate(physics.DummyUpdate);
            map = new Map();
        }

        public void OfflineGameUpdate(Camera camera, GameTime gameTime)
        {
            if (map.Objects.Count == 0)
                return;
                
            physicsUpdate(gameTime);
            map.Update(camera, gameTime);
        }

        public void ServerGameUpdate(Camera camera, GameTime gameTime)
        {
            engine.NetworkServer.Update();
            physicsUpdate(gameTime);
            map.Update(camera, gameTime);
        }

        public void ClientGameUpdate(Camera camera, GameTime gameTime)
        {
            engine.NetworkClient.Update();
            physicsUpdate(gameTime);
            map.Update(camera, gameTime);
        }

        public void EditorUpdate(Camera camera, GameTime gameTime)
        {
            if (physics.SimulationUpdate)
                physicsUpdate(gameTime);

            map.Update(camera, gameTime);
        }

        public void Draw(Camera camera, GameTime gameTime)
        {
            foreach (var r in rp.Values)
                DrawReflectionParameters(r);

            graphics.Update(camera, gameTime);
            graphics.Draw(graphics.OutputTexture, camera);
        }

        private Dictionary<string, ReflectionParameters> rp = new Dictionary<string, ReflectionParameters>();

        public Dictionary<string, ReflectionParameters> ReflectionParameters => rp;

        internal void SetReflectionParams(string name, ReflectionParameters reflectionParameters)
        {
            if (rp.ContainsKey(name))
                rp[name] = reflectionParameters;
            else
                rp.Add(name, reflectionParameters);
        }

        internal void DrawReflectionParameters(ReflectionParameters r)
        {
            Camera camera = engine.Camera;
            camera.Update(camera.Transform * r.RelfectionMatrix);
            graphics.UpdateVisibleDeferredObjects(camera);

            graphics.PreDrawReflection(camera, r.RelfectionMatrix, r.ClipPlane);
            graphics.DrawReflection(r.GBuffer, r.LBuffer, r.RenderTarget, camera);
            camera.Update(camera.Transform);
            graphics.PostDrawReflection(camera, r.RelfectionMatrix, Vector4.Zero);
            graphicsDevice.SetRenderTarget(null);
        }

        #region CreateMapObject

        public T CreateMapObject<T>(string name) where T : MapObject, new()
        {
            T instance = new T
            {
                Name = name ?? RandomString.AlphaNumeric(8),
                uid = map.GenerateRandomUID()
            };

            map.QueueObjectAdd(instance);
            return instance;
        }

        public T CreateMapObject<T>(string name, string gameObjectPath) where T : MapObject, new()
        {
            T instance = new T
            {
                Name = name ?? RandomString.AlphaNumeric(8),
                GameObjectPath = gameObjectPath,
                uid = map.GenerateRandomUID()
            };

            map.QueueObjectAdd(instance);
            return instance;
        }

        public T CreateMapObject<T>(string name, string gameObjectPath, Vector3 translation) where T : MapObject, new()
        {
            T instance = new T
            {
                Name = name ?? RandomString.AlphaNumeric(8),
                GameObjectPath = gameObjectPath,
                InitialTransform = WorldTransform3.Create(translation),
                uid = map.GenerateRandomUID()
            };

            map.QueueObjectAdd(instance);
            return instance;
        }

        public T CreateMapObject<T>(string name, string gameObjectPath, Quaternion rotation, Vector3 translation) where T : MapObject, new()
        {
            T instance = new T
            {
                Name = name ?? RandomString.AlphaNumeric(8),
                GameObjectPath = gameObjectPath,
                InitialTransform = WorldTransform3.Create(rotation, translation),
                uid = map.GenerateRandomUID()
            };

            map.QueueObjectAdd(instance);
            return instance;
        }

        public T CreateMapObject<T>(string name, string gameObjectPath, Vector3 scale, Quaternion rotation, Vector3 translation) where T : MapObject, new()
        {
            T instance = new T
            {
                Name = name ?? RandomString.AlphaNumeric(8),
                GameObjectPath = gameObjectPath,
                InitialTransform = WorldTransform3.Create(scale, rotation, translation),
                uid = map.GenerateRandomUID()
            };

            map.QueueObjectAdd(instance);
            return instance;
        }

        public MapObject CreateMapObject(string name, string gameObjectPath, ushort uid, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            MapObject mo = new MapObject(gameObjectPath, name, uid)
            {
                InitialTransform = WorldTransform3.Create(scale, rotation, position)
            };

            switch (engine.NetworkType)
            {
                case Network_User_Type.Server:
                    {
                        //only add to the queue when a server. clients will add to queue
                        //once we inform them the server object has been added to the servers scene
                        map.QueueObjectAdd(mo);

                        var msg = engine.NetworkServer.CreateMessageHeader(Custom_Message_Type.Map);
                        msg.Write((ushort)Map.Network_Message.ObjectAddedToQueue);
                        msg.Write(gameObjectPath);
                        msg.Write(name);
                        msg.Write(uid);
                        msg.Write(position);
                        msg.Write(rotation);

                        engine.NetworkServer.NetworkObject.SendToAll(msg, Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                    }
                    break;
                case Network_User_Type.Offline:
                    map.QueueObjectAdd(mo);
                    break;
            }

            return mo;
        }

        //Creates a map object on the local map. Should be used to create client side only objects on client maps
        //Offline users can use it too as it is a little less code to create a map object in an offline map
        public MapObject CreateMapObject(string name, string gameObjectPath, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null, bool addToScene = true)
        {
            MapObject mo = new MapObject(gameObjectPath, name ?? RandomString.AlphaNumeric(8), map.GenerateRandomUID())
            {
                InitialTransform = WorldTransform3.Create(scale ?? Vector3.One, rotation ?? Quaternion.Identity, position ?? Vector3.Zero)
            };

            if (addToScene)
                map.QueueObjectAdd(mo);

            return mo;
        }

        #endregion

        #region LoadMap

        public Map LoadMap(string filePath)
        {
            //switch out to the dummy update delegate to stop updating the physics simulation
            physicsUpdate = new PhysicsUpdateDelegate(physics.DummyUpdate);

            var newMap = ContentLoader.LoadMap(engine, filePath);

            var callback = engine.Interface.CreateDefaultLoadingScreen(newMap.ObjectCount);

            Task.Run(() => {
#pragma warning disable CS4014
                newMap.LoadAsync(engine, callback, (m) =>
                {
                    Log.Instance.Write("Map unloaded");
                    map.Unload();
                    map = m;
                    physicsUpdate = new PhysicsUpdateDelegate(physics.TimestepUpdate);
                    MapLoaded?.Invoke(filePath, m);
                });
#pragma warning restore CS4014
            });

            return newMap;
        }

        #endregion

        public void Destroy(bool destroyAv)
        {
            map.Unload();
            graphics.DestroyRenderables();
            physics.SimulationUpdate = false;
            physics.Destroy();

            if (destroyAv)
            {
                graphics.Destroy();
                graphics = null;
            }
        }
    }
}
