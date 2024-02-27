using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Helpers;
using Engine.AssetTransport;
using Engine.Content;
using Engine.Content.Model;
using Engine.Editor.Components;
using Engine.Graphics.Effects;
using Engine.Helpers;
using Engine.World.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.OpenGL;

namespace Engine.Editor.Views
{
    public class TextureCubeView : View
    {
        private TextureCubeEffect effect;
        private int currentMipLevel = 0;
        private List<TextureCube> asset;
        private SharedMeshData meshData;

        protected EditorCameraController cameraController;
        protected bool updateCameraController = true;

        public bool SceneLoaded => asset != null && effect != null && meshData != null;

        public override void InitializeView(string path)
        {
            engine.Camera.Controller = new EditorCameraController();
            cameraController = (EditorCameraController)engine.Camera.Controller;

            cameraController.Position = Vector3.Zero;
            
            Task.Run(async () => {
                asset = await TextureCubeReader.ReadMipmapsAsync(engine.GraphicsDevice, File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal(path))).ConfigureAwait(false);
            });
            Task.Run(async () => {
                effect = new TextureCubeEffect();
                await effect.LoadAsync(engine.GraphicsDevice).ConfigureAwait(false);
                effect.Texture = asset[0];
                effect.World = Matrix4x4.Identity;
            });
            Task.Run(async () => {
                await new AsyncUiTask().Run(() => {
                    meshData = Cube.CreateMeshData(engine.GraphicsDevice);
                });
            });
        }
        
        public virtual bool ShouldUpdateCameraController()
        {
            if (engine.Interface.Terminal.Visible)
                return false;

            return true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!SceneLoaded)
                return;

            if (engine.Interface.Terminal.Visible)
                return;

            updateCameraController = ShouldUpdateCameraController();

            if (updateCameraController)
                engine.Camera.Controller.Update(gameTime);

            updateCameraController = true;

            engine.Interface.ScreenMessages.WriteStaticText(1, $" Total mipmap levels: {asset.Count}", Color.White);
            engine.Interface.ScreenMessages.WriteStaticText(2, $"Current mipmap level: {currentMipLevel}", Color.White);

            int newMipLevel = currentMipLevel;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.Down))
                ++newMipLevel;

            if (engine.Input.Keyboard.KeyJustPressed(Keys.Up))
                --newMipLevel;

            newMipLevel = MathHelper.Clamp(newMipLevel, 0, asset.Count - 1);

            if (newMipLevel != currentMipLevel)
            {
                currentMipLevel = newMipLevel;
                effect.Texture = asset[currentMipLevel];
            }

            cameraController.Position = Vector3.Zero;
        }

        public override void Draw(GameTime gameTime)
        {
            engine.GraphicsDevice.SetRenderTarget(null);
            engine.GraphicsDevice.Clear(GraphicsDevice.DiscardDefault);

            if (!SceneLoaded)
                return;

            engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            effect.View = engine.Camera.View;
            effect.Projection = engine.Camera.Projection;

            engine.GraphicsDevice.SetVertexBuffers(meshData.ReconstructBinding);
            engine.GraphicsDevice.SetIndexBuffer(meshData.IndexBuffer);

            effect.Apply();

            engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 36);
            engine.Interface.Draw();
        }
    }
}
