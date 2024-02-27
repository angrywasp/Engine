using Engine.Content;
using Engine.Interfaces;
using Engine.Scene;
using Microsoft.Xna.Framework.Graphics;
using Engine.Cameras;
using MonoGame.OpenGL;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework;
using AngryWasp.Random;
using Engine.Graphics.Vertices;
using Engine.Content.Model;

namespace Engine.World.Objects
{
    public class Cube
    {
        private const float p = 0.5f;

        private static readonly VertexPositionTexture[] positions =
        {
            new VertexPositionTexture(new Vector3(-p, p, p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-p, -p, p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(p, -p, p), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(p, p, p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-p, p, -p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(p, p, -p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(p, -p, -p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-p, -p, -p), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-p, p, p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(p, p, p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(p, p, -p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-p, p, -p), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(p, p, p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(p, -p, p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(p, -p, -p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(p, p, -p), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(p, -p, p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-p, -p, p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-p, -p, -p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(p, -p, -p), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(-p, -p, p), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-p, p, p), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-p, p, -p), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-p, -p, -p), new Vector2(0, 0)),
        };

        private static readonly int[] indices = { 0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4, 8, 9, 10, 10, 11, 8, 12, 13, 14, 14, 15, 12, 16, 17, 18, 18, 19, 16, 20, 21, 22, 22, 23, 20 };

        public static SharedMeshData CreateMeshData(GraphicsDevice graphicsDevice)
        {
            var v = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), positions.Length, BufferUsage.None);
            v.SetData(positions);

            var i = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
            i.SetData(indices);

            return new SharedMeshData(graphicsDevice, positions, indices);
        }
    }
    
    public class SkyboxType : GameObjectType
    {
        [JsonProperty] public string EnvironmentMap { get; set; }
        [JsonProperty] public string IrradianceMap { get; set; }
        [JsonProperty] public string ReflectanceMap { get; set; }
    }

    public class Skybox : GameObject, IDrawableObjectDeferred
    {
        private SkyboxType _type = null;

        public new SkyboxType Type => _type;

        private SkyboxMaterial material;

        private int key;
        private SharedMeshData sharedMeshData;

        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine);

            await new AsyncUiTask().Run(() => {
                key = RandomString.AlphaNumeric(8).GetHashCode();
                sharedMeshData = Cube.CreateMeshData(engine.GraphicsDevice);
            }).ConfigureAwait(false);
            
            material = new SkyboxMaterial
            {
                AlbedoMap = _type.EnvironmentMap
            };
            
            await material.LoadAsync(engine).ConfigureAwait(false);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            transform.Update(Matrix4x4.CreateTranslation(camera.Position));
            material.UpdateTransform(transform.Matrix, camera.View, camera.Projection);
            
            foreach (var x in Components.Values)
                x.Update(camera, gameTime);
        }

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            engine.Scene.Graphics.AmbientLightEffect.IrradianceMap = _type.IrradianceMap;
            engine.Scene.Graphics.AmbientLightEffect.PreFilterMap = _type.ReflectanceMap;
            engine.Scene.Graphics.AddDrawable(key, sharedMeshData, this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            engine.Scene.Graphics.RemoveDrawable(key, this);
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Deferred;

        public bool ShouldDraw(Camera camera) => true;

        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer) { }

        RasterizerState rs = RasterizerState.CullCounterClockwise;

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullClockwise;
            material.Clipping.Enable(plane.W);
            material.UpdateTransform(transform.Matrix, camera.View, camera.Projection);
        }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullCounterClockwise;
            material.Clipping.Disable();
            material.UpdateTransform(transform.Matrix, camera.View, camera.Projection);
        }

        #endregion

        #region IDrawableObjectDeferred implementation

        public void RenderToGBuffer(Camera camera)
        {
            engine.GraphicsDevice.RasterizerState = rs;
            material.Apply("RenderToGBuffer");
            engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 36);
        }

        public void ReconstructShading(Camera camera)
        {
            engine.GraphicsDevice.RasterizerState = rs;
            material.Apply("Reconstruct");
            engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, 0, 0, 36);
        }

        #endregion
    }
}
