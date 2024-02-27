using System.Diagnostics;
using Engine.Configuration;
using Engine.Cameras;
using Engine.World;
using Engine.Input;
using Engine.Multiplayer;
using Engine.PostProcessing;
using Engine.Scene;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;
using AngryWasp.Logger;
using AngryWasp.Helpers;
using MonoGame.OpenGL;
using AngryWasp.Random;

namespace Engine
{
    public delegate void EngineEventHandler<Tsender>(Tsender sender);
    public delegate void EngineEventHandler<Tsender, Targs>(Tsender sender, Targs args);

    public class EngineCore
    {
        private delegate void UpdateDelegate(Camera camera, GameTime gameTime);
        private UpdateDelegate update;
        private NetworkClient nc;
        private NetworkServer ns;
        private Network_User_Type networkType = Network_User_Type.Offline;
        private XoShiRo128PlusPlus random = new XoShiRo128PlusPlus();

        private GraphicsDevice graphicsDevice;
        private InputDeviceManager inputDeviceManager;
        private Interface ui;
        private EngineScene scene;
        private Camera camera;
        private PostProcessManager postProcessor;
        private TypeFactory typeFactory;
        private FrameCounter frameCounter = new FrameCounter();

        public GraphicsDevice GraphicsDevice => graphicsDevice;

        public InputDeviceManager Input => inputDeviceManager;

        public Camera Camera => camera;

        public EngineScene Scene => scene;

        public PostProcessManager PostProcessor => postProcessor;

        public TypeFactory TypeFactory => typeFactory;

        public FrameCounter Counter => frameCounter;

        public NetworkClient NetworkClient => nc;

        public NetworkServer NetworkServer => ns;

        public Network_User_Type NetworkType => networkType;

        public XoShiRo128PlusPlus Random => random;

        public Interface Interface => ui;

        public bool NetworkConnected => nc != null && nc.IsConnected;

        public bool IsEditor => true; //todo: implement

        private static EngineCore instance;

        public static EngineCore Instance => instance;

        public EngineCore(GraphicsDevice g, string typeAssembly)
        {
            instance = this;

            if (typeAssembly != null)
                ReflectionHelper.Instance.LoadAssemblyFile(typeAssembly);

            typeFactory = new TypeFactory(this);

            graphicsDevice = g;

            camera = new Camera(this);

            scene = new EngineScene(this);
            inputDeviceManager = new InputDeviceManager(graphicsDevice);

            //uiRenderer = new UIRenderer(graphicsDevice);
            ui = new Interface(graphicsDevice, inputDeviceManager);

            postProcessor = new PostProcessManager(this);

            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GraphicsExtensions.CheckGLError();

            //update = new UpdateDelegate(scene.OfflineGameUpdate);
        }

        public void Update(GameTime gameTime)
        {
            inputDeviceManager.Update(gameTime);
            camera.Update();
            ui.Update(gameTime);

            if (update != null)
                update(camera, gameTime);
        }

        public void Resize(int w, int h)
        {
            Threading.BlockOnUIThread(() =>
            {
                Viewport viewport = new Viewport(0, 0, w, h);

                graphicsDevice.Viewport = viewport;
                PresentationParameters pp = graphicsDevice.PresentationParameters;
                pp.BackBufferWidth = w;
                pp.BackBufferHeight = h;
                graphicsDevice.Reset(pp);

                scene.Graphics.CreateRenderTargets(w, h);

                ui.Resize(0, 0, w, h);
                camera.Resize();
                postProcessor.Resize(w, h);
            });
        }

        public void Shutdown()
        {
            if (scene != null)
                scene.Destroy(false);

            Settings.Save();

            if (networkType == Network_User_Type.Server)
                ns.NetworkObject.Shutdown("bye");
            else if (networkType == Network_User_Type.Client)
                nc.NetworkObject.Disconnect("bye");

            GraphicsDevice.Reset();
        }

        public void CreateNetworkClient(int port, string privateKey)
        {
            if (nc != null)
            {
                //todo: Problem in code. nc should always be null when this method is called
                //we should check if already connected and disconnect first
                Debugger.Break();
            }

            networkType = Network_User_Type.Client;
            nc = new NetworkClient(this, "127.0.0.1"/*MultiplayerHelper.GetLocalNetworkAddress()*/, port, privateKey);
            update = new UpdateDelegate(scene.ClientGameUpdate);
            nc.NetworkObject.Start();
        }

        public void CreateNetworkServer(int port, string privateKey, int maxConnections, string mapPath)
        {
            if (ns != null)
                Debugger.Break();

            networkType = Network_User_Type.Server;
            ns = new NetworkServer(this, "127.0.0.1"/*MultiplayerHelper.GetLocalNetworkAddress()*/, port, privateKey, maxConnections, mapPath);
            update = new UpdateDelegate(scene.ServerGameUpdate);
            ns.NetworkObject.Start();

            if (ns.NetworkObject.Status == NetPeerStatus.Running)
                Log.Instance.Write($"Server is running on port {port}");
            else
                Log.Instance.Write("Server not started...");
        }

        public void Draw(Camera camera, GameTime gameTime)
        {
            postProcessor.Update(camera, gameTime);
            postProcessor.Draw(scene.Graphics.OutputTexture);
            graphicsDevice.BlendState = BlendState.Opaque;
            ui.DrawRectangle(postProcessor.RenderTarget, Color.White, Vector2i.Zero, ui.Size);
            ui.Draw();
        }
    }
}
