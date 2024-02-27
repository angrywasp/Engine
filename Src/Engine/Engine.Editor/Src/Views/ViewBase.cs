using Engine.Editor.Components;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using Engine.Editor.Forms;
using Microsoft.Xna.Framework.Input;
using AngryWasp.Logger;
using System;
using Engine.Editor.Components.UI;
using Engine.Content;
using System.Diagnostics;
using Engine.PostProcessing.AA;
using AngryWasp.Random;
using Engine.World;
using Engine.World.Components.Lights;
using Engine.World.Objects;
using Engine.World.Components;
using Microsoft.Xna.Framework.Graphics;
using Engine.Configuration;

namespace Engine.Editor.Views
{
    public interface IView
    {
        bool IsInitialized { get; }
        string AssetPath { get; }

        void Init(string path);
        void UnInitialize();
        void Resize(int w, int h);
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }

    public abstract class View : IView
    {
        protected EngineCore engine;
        protected string assetPath;
        private bool isInitialized = false;

        public string AssetPath => assetPath;

        public bool IsInitialized => isInitialized;

        protected EditorForm activeEditorForm;

        private int renderTargetCounter = 0;
        private int renderTargetCounterMax = 7;

        protected DirectionalLightComponent dl;

        public void Init(string path)
        {
            Threading.BlockOnUIThread(() =>
            {
                try
                {
                    string typeAssemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Engine.Objects.dll");
                    engine = new EngineCore(EditorGame.Instance.GraphicsDevice, typeAssemblyPath);

                    Resize(EditorGame.Instance.Window.ClientBounds.Width, EditorGame.Instance.Window.ClientBounds.Height);

                    engine.PostProcessor.Add("aa", new SMAA());

                    engine.Scene.Graphics.DebugDraw = true;
                    engine.Scene.Physics.SimulationUpdate = true;

                    this.assetPath = path;
                    InitializeView(path);
                    isInitialized = true;
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteException(ex);
                    Debugger.Break();
                }
            });
        }

        public abstract void InitializeView(string path);

        public virtual void UnInitialize()
        {
            Threading.BlockOnUIThread(() =>
            {
                engine.Shutdown();
                isInitialized = false;
            });
        }

        public void Resize(int w, int h) => engine.Resize(w, h);

        int aaCurrent = 0;
        int aaMax = 3;

        private void SwitchSMAAEdgeDetectionMethod()
        {
            var smaa = engine.PostProcessor["aa"];
            if (smaa == null)
                return;

            Log.Instance.Write($"Using SMAA edge detection mode {((Edge_Detection_Method)aaCurrent).ToString()}");
            ((SMAA)smaa).Backend.SetEdgeDetectionMethod((Edge_Detection_Method)aaCurrent);
        }

        public virtual async void Update(GameTime gameTime)
        {
            engine.Update(gameTime);
            engine.Scene.EditorUpdate(engine.Camera, gameTime);

            if (activeEditorForm != null)
                activeEditorForm.ViewUpdate(gameTime);

            if (engine.Input.Keyboard.KeyJustPressed(Keys.M))
                engine.Scene.Physics.SimulationUpdate = !engine.Scene.Physics.SimulationUpdate;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.P))
            {
                if (engine.PostProcessor["aa"] == null)
                {
                    var smaa = new SMAA();
                    smaa.Loaded += (p) =>
                    {
                        Log.Instance.Write("SMAA enabled");
                        SwitchSMAAEdgeDetectionMethod();
                    };

                    engine.PostProcessor.Insert(0, "aa", smaa);
                }
                else
                {
                    Log.Instance.Write("SMAA disabled");
                    engine.PostProcessor.Remove("aa");
                }
            }

            if (engine.Input.Keyboard.KeyJustPressed(Keys.L))
            {
                if (engine.PostProcessor["aa"] != null)
                {
                    var smaa = (SMAA)engine.PostProcessor["aa"];
                    ++aaCurrent;

                    if (aaCurrent >= aaMax)
                        aaCurrent = 0;

                    SwitchSMAAEdgeDetectionMethod();
                }
            }

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.OemTilde))
            {
                engine.Interface.Terminal.ResetCommandHistory();
                engine.Interface.Terminal.Visible = !engine.Interface.Terminal.Visible;
            }

            if (engine.Interface.Terminal.Visible)
                return;

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.R))
            {
                ++renderTargetCounter;
                if (renderTargetCounter == renderTargetCounterMax)
                    renderTargetCounter = 0;
            }

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
                engine.Scene.Graphics.DebugDraw = !engine.Scene.Graphics.DebugDraw;

            if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.NumPad1))
            {
                var fileBrowserResult = await new FileBrowser(engine.Interface).Show().ConfigureAwait(false);
                var nodeType = NodeType.Unknown;

                if (!string.IsNullOrEmpty(fileBrowserResult.Path))
                    nodeType = Engine.Editor.Helpers.GetNodeTypeFromExtension(Path.GetExtension(fileBrowserResult.Path).Trim('.').ToLower());

                if (nodeType == NodeType.Unknown)
                    return;

                AssetLoader.Load(fileBrowserResult.Path);
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
            FPSCounter.Update(gameTime);
            engine.Scene.Draw(engine.Camera, gameTime);
            if (activeEditorForm != null)
                activeEditorForm.ViewDraw();
            engine.Draw(engine.Camera, gameTime);
            engine.Interface.ScreenMessages.WriteStaticText(0, $"FPS: {FPSCounter.FramePerSecond}", FPSCounter.FramePerSecond < Settings.Engine.TargetFPS ? Color.Red : Color.Green);

            /*engine.GraphicsDevice.BlendState = BlendState.Opaque;

            var font = engine.Interface.DefaultFont.GetByFontSize(36);
            switch (renderTargetCounter)
            {
                case 0:
                    engine.Interface.DrawRectangle(engine.PostProcessor.RenderTarget, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Final", new Vector2i(10, 50), font, Color.White);
                    break;
                case 1:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.GBuffer.Albedo, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Albedo", new Vector2i(10, 50), font, Color.White);
                    break;
                case 2:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.GBuffer.Normal, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Normal", new Vector2i(10, 50), font, Color.White);
                    break;
                case 3:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.GBuffer.PBR, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("PBR", new Vector2i(10, 50), font, Color.White);
                    break;
                case 4:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.GBuffer.Depth, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Depth", new Vector2i(10, 50), font, Color.White);
                    break;
                case 5:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.LBuffer.Radiance, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Radiance", new Vector2i(10, 50), font, Color.White);
                    break;
                case 6:
                    engine.Interface.DrawRectangle(engine.Scene.Graphics.LBuffer.Ambient, Color.White, Vector2i.Zero, engine.Interface.Size);
                    //engine.Interface.DrawString("Ambient", new Vector2i(10, 50), font, Color.White);
                    break;
            }

            //engine.Interface.DrawDeferredText();
            engine.Interface.Draw();*/
        }

        protected void ShowForm(EditorForm form)
        {
            form.Show();
            activeEditorForm = form;
        }
    }

    public abstract class View3D : View
    {
        //private TessellatedMeshMaterial tessellatedMeshMaterial;
        //private MeshMaterial standardMeshMaterial;
        //private Mesh tesselatedMesh;

        protected EditorCameraController cameraController;
        protected bool updateCameraController = true;

        public override void InitializeView(string path)
        {
            engine.Camera.Controller = new EditorCameraController();
            cameraController = (EditorCameraController)engine.Camera.Controller;
        }

        public virtual Map CreateDefaultScene() => engine.Scene.LoadMap("DemoContent/Maps/DefaultEditorMap.map");


        /*{
            tessellatedMeshMaterial = new TessellatedMeshMaterial();
            standardMeshMaterial = new MeshMaterial();

            tessellatedMeshMaterial.DiffuseMap = standardMeshMaterial.DiffuseMap = "Engine/Textures/Default_albedo.texture";
            tessellatedMeshMaterial.NormalMap = standardMeshMaterial.NormalMap = "Engine/Textures/Default_normal.texture";
            tessellatedMeshMaterial.PbrMap = "DemoContent/Materials/Textures/Grid_pbr.texture";
            await tessellatedMeshMaterial.LoadAsync(engine).ConfigureAwait(false);
            await standardMeshMaterial.LoadAsync(engine).ConfigureAwait(false);

            tesselatedMesh = await ContentLoader.LoadMesh(engine, "Engine/Renderer/Meshes/Cube.mesh").ConfigureAwait(false);
        }*/

        /*public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //engine.GraphicsDevice.SetRenderTarget(engine.Scene.Graphics.OutputTexture);

            if (tesselatedMesh == null || tessellatedMeshMaterial == null)
                return;

            engine.GraphicsDevice.SetVertexBuffers(new VertexBufferBinding[] {
                new VertexBufferBinding(tesselatedMesh.PositionVertexBuffer),
                new VertexBufferBinding(tesselatedMesh.NormalVertexBuffer)
            });

            engine.GraphicsDevice.Indices = tesselatedMesh.IndexBuffer;

            engine.GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            //engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            var s = tesselatedMesh.SubMeshes[0];

            standardMeshMaterial.UpdateTransform(Matrix4x4.CreateTranslation(new Vector3(-1, 1, 0)), engine.Camera.View, engine.Camera.Projection);
            tessellatedMeshMaterial.UpdateTransform(Matrix4x4.CreateTranslation(new Vector3(1, 1, 0)), engine.Camera.View, engine.Camera.Projection);

            tessellatedMeshMaterial.Apply(0);
            engine.GraphicsDevice.DrawIndexedPrimitives(MonoGame.OpenGL.GLPrimitiveType.Patches, s.BaseVertex, s.StartIndex, s.PrimitiveCount);

            standardMeshMaterial.Apply(0);
            engine.GraphicsDevice.DrawIndexedPrimitives(MonoGame.OpenGL.GLPrimitiveType.Triangles, s.BaseVertex, s.StartIndex, s.PrimitiveCount);
        }*/

        public virtual bool ShouldUpdateCameraController()
        {
            if (engine.Interface.Terminal.Visible)
                return false;

            if (engine.Input.Keyboard.KeyDown(Keys.LeftControl))
                return false;

            if (activeEditorForm != null)
                return activeEditorForm.ShouldUpdateCameraController();

            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (engine.Interface.Terminal.Visible)
                return;

            updateCameraController = ShouldUpdateCameraController();

            if (updateCameraController)
            {
                engine.Camera.Controller.Update(gameTime);
                if (engine.Input.Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.B))
                    ShootBall();
            }

            updateCameraController = true;
        }

        protected string[] colors = { "Blue", "Cyan", "Gray", "Magenta", "Orange", "Pink", "Purple", "Red", "Yellow" };

        internal void ShootBall()
        {
            Ray r = engine.Input.Mouse.ToRay(engine.Camera.View, engine.Camera.Projection);
            
            var mo = engine.Scene.CreateMapObject<RuntimeMapObject>(null, "DemoContent/Entities/Sphere/DynamicSphere.type", r.Position);
            mo.Loaded += async (mapObject, gameObject) =>
            {
                var mat = ContentLoader.LoadMaterial($"Engine/Materials/Colors/{colors[engine.Random.Next(0, colors.Length)]}.material", false);
                await mat.LoadAsync(engine).ConfigureAwait(false);

                ((MeshComponent)gameObject.Components["Mesh_0"]).Mesh.SetMaterial(mat, 0);

                var s = gameObject.PhysicsModel.Shapes[0];
                var b = engine.Scene.Physics.Simulation.Bodies[s.BodyHandle];
                b.Pose.Position = r.Position;
                b.Velocity.Linear = (r.Direction * 50) * s.InverseMass;
            };
        }
    }
}
