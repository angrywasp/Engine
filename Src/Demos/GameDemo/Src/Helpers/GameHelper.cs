using Engine;
using Engine.Multiplayer;
using Engine.World;
using Engine.World.Objects;

namespace GameDemo.Helpers
{
    public class GameHelper
    {
        private EngineCore engine;

        public GameHelper(EngineCore engine)
        {
            this.engine = engine;
        }

        private bool mapLoaded = false;
        public void LoadMap()
        {
            switch (engine.NetworkType)
            {
                case Network_User_Type.Offline:
                    {
                        BasicGame.Instance.IsMouseVisible = false;
                        engine.Scene.MapLoaded += (_, _) => {
                            PlayerManager.Instance.AvatarCreated += playerManager_AvatarCreated;
                            PlayerManager.Instance.QueuePlayerAdd();
                        };
                        
                        var map = engine.Scene.LoadMap(Engine.Configuration.Settings.Engine.ServerHost.Map);
                    }
                    break;
                case Network_User_Type.Client:
                    {
                        engine.NetworkClient.RequestMapFile();
                        engine.NetworkClient.MapFile += delegate (bool isServerMapLoaded, string mapFile)
                        {
                            //load the map regardless of whether or not the server is ready

                            //when the client connects, it automatically asks the server what map file it is running
                            //so the first time we get this message, the map could possibly not be loaded on the server
                            //but if this is the case, we will get a second message when it is
                            //so we can load the map the first time this message is received (after joining the game)
                            //and only request a map update when the server map is loaded, as indicated by isServerMapLoaded

                            //load the map, but only once
                            if (!mapLoaded)
                            {
                                engine.Scene.LoadMap(mapFile);
                                PlayerManager.Instance.AvatarCreated += playerManager_AvatarCreated;
                                mapLoaded = true;
                            }

                            //if the server's map is loaded and running, get an update
                            if (isServerMapLoaded)
                            {
                                //this is the message we have received when the server map has loaded. this only happens when the server map is loaded
                                //as a message to the clients already connected, in which case, ask for a map update
                                engine.NetworkClient.RequestInitialMapUpdate();
                            }
                        };
                    }
                    break;
                case Network_User_Type.Server:
                    {
                        BasicGame.Instance.IsMouseVisible = false;
                        engine.Scene.LoadMap(Engine.Configuration.Settings.Engine.ServerHost.Map);
                        engine.Camera.Controller = new EditorCameraController(); //server cannot participate in the game, only watch
                        //todo: need overwatch view
                        //BasicGame.Instance.LoadView(new ActionView());
                    }
                    break;
            }
        }

        private void playerManager_AvatarCreated(MapObject avatar, MapObject controller)
        {
            PlayerManager.Instance.AvatarCreated -= playerManager_AvatarCreated;
            BasicGame.Instance.LoadView(new ActionView(avatar, controller));
            BasicGame.Instance.IsMouseVisible = false;
        }
    }
}