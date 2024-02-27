using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using AngryWasp.Logger;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Engine.Cameras;
using Engine.Content;
using Engine.Content.Terrain;
using Engine.Graphics.Materials;
using Engine.Interfaces;
using Engine.Physics;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.World.Objects
{
    public class HeightmapTerrainType : GameObjectType
    {
        [JsonProperty] public string DiffuseMap { get; set; }
        [JsonProperty] public string NormalMap { get; set; }

        [JsonProperty] public string DiffuseMap0 { get; set; }
        [JsonProperty] public string NormalMap0 { get; set; }

        [JsonProperty] public string DiffuseMap1 { get; set; }
        [JsonProperty] public string NormalMap1 { get; set; }

        [JsonProperty] public string DiffuseMap2 { get; set; }
        [JsonProperty] public string NormalMap2 { get; set; }

        [JsonProperty] public string DiffuseMap3 { get; set; }
        [JsonProperty] public string NormalMap3 { get; set; }

        [JsonProperty] public string BlendTexture { get; set; }
        [JsonProperty] public Vector2 TextureScale { get; set; } = Vector2.One;
        [JsonProperty] public string HeightMap { get; set; }
    }

    public class HeightmapTerrain : GameObject, IDrawableObjectDeferred
    {
        private HeightmapTerrainType _type = null;

        public new HeightmapTerrainType Type => _type;

        private TerrainMaterial material;
        private HeightmapData heightmap;
        private string heightmapPath;
        private List<Vector3> vertexData;
        private QTNode rootNode;
        private int key;

        public QTNode QuadTree => rootNode;

        public Texture2D BlendTexture => material.BlendTexture;

        public TerrainMaterial Material => material;

        public HeightmapData Heightmap => heightmap;

        public List<Vector3> VertexData => vertexData;


        public override async Task LoadAsync(EngineCore engine)
        {
            await base.LoadAsync(engine).ConfigureAwait(false);

            material = new TerrainMaterial
            {
                DiffuseMapPath = _type.DiffuseMap,
                NormalMapPath = _type.NormalMap,
                DiffuseMap0Path = _type.DiffuseMap0,
                NormalMap0Path = _type.NormalMap0,
                DiffuseMap1Path = _type.DiffuseMap1,
                NormalMap1Path = _type.NormalMap1,
                DiffuseMap2Path = _type.DiffuseMap2,
                NormalMap2Path = _type.NormalMap2,
                DiffuseMap3Path = _type.DiffuseMap3,
                NormalMap3Path = _type.NormalMap3,
                BlendTexturePath = _type.BlendTexture,
                TextureScale = _type.TextureScale,
            };
            
            await material.LoadAsync(engine).ConfigureAwait(false);

            var sw = new Stopwatch();
            sw.Start();

            if (string.IsNullOrEmpty(_type.HeightMap))
                return;

            if (heightmapPath != _type.HeightMap || heightmap == null)
            {
                heightmapPath = _type.HeightMap;
                heightmap = ContentLoader.LoadHeightmap(engine.GraphicsDevice, _type.HeightMap);

                await Task.WhenAll(
                    Task.Run(() =>
                    {
                            var sw1 = new Stopwatch();
                            sw1.Start();
                            Log.Instance.Write("Started generating terrain quad tree");
                            rootNode = new QTNode(engine, heightmap.Positions, heightmap.Normals, engine.GraphicsDevice, 64);
                            sw1.Stop();
                            Log.Instance.Write($"Finished generating terrain quad tree in {sw1.ElapsedMilliseconds} ms");
                    }),
                    Task.Run(() =>
                    {
                            var sw2 = new Stopwatch();
                            sw2.Start();
                            Log.Instance.Write("Started generating terrain physics");
                            CreateTerrainPhysics(heightmap.Width, heightmap.Width, (int vX, int vY) =>
                            {
                                return new Vector3(vX, heightmap.HeightData[vX, vY], vY);
                            },
                            new Vector3(1, 1, 1), engine.Scene.Physics.DefaultBufferPool, out var planeMesh, out vertexData);

                            var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(planeMesh);

                            engine.Scene.Physics.AddStaticBody(
                                new StaticDescription(new Vector3(0, 0, 0), shapeIndex),
                                new Material(30, 1, 1, float.MaxValue).ToPhysicsMaterial()
                            );
                            sw2.Stop();
                            Log.Instance.Write($"Finished generating terrain physicsin {sw2.ElapsedMilliseconds} ms");
                    })
                ).ConfigureAwait(false);

                rootNode.UpdateBoundingBox(Matrix4x4.Identity);
            }

            key = $"terrain_{this.Name}".GetHashCode();

            sw.Stop();
            Log.Instance.Write($"Terrain loaded in {sw.ElapsedMilliseconds} ms");
        }

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();
            engine.Scene.Graphics.AddDrawable(key, null, this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            engine.Scene.Graphics.RemoveDrawable(key, this);
        }

        public override void Update(Camera camera, GameTime gameTime)
        {
            QTNode.NodesVisible = 0;
            rootNode.Update(Matrix4x4.Identity, camera.Frustum);
            material.UpdateTransform(Matrix4x4.Identity, camera.View, camera.Projection);
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Deferred;

        public bool ShouldDraw(Camera camera) => true;

        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer) => material.SetBuffers(lBuffer, gBuffer);

        RasterizerState rs = RasterizerState.CullCounterClockwise;

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullClockwise;
            rootNode.Update(Matrix4x4.Identity, camera.Frustum);
            material.Clipping.Enable(plane.W);
            material.UpdateTransform(Matrix4x4.Identity, camera.View, camera.Projection);
        }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullCounterClockwise;
            rootNode.Update(Matrix4x4.Identity, camera.Frustum);
            material.Clipping.Disable();
            material.UpdateTransform(Matrix4x4.Identity, camera.View, camera.Projection);
        }

        public void DebugDraw(Camera camera, GameTime gameTime) { }

        #endregion

        #region IDrawableObjectDeferred implementation

        public void RenderToGBuffer(Camera camera)
        {
            engine.GraphicsDevice.RasterizerState = rs;
            material.Apply("RenderToGBuffer");
            rootNode.Draw(true, false, Matrix4x4.Identity, engine.Camera.Frustum);
        }

        public void ReconstructShading(Camera camera)
        {
            material.Apply($"Reconstruct");
            rootNode.Draw(false, true, Matrix4x4.Identity, engine.Camera.Frustum);
        }

        #endregion

        private void CreateTerrainPhysics(int width, int height, Func<int, int, Vector3> deformer, Vector3 scaling, BufferPool pool, out BepuPhysics.Collidables.Mesh mesh, out List<Vector3> vertexList)
        {
            pool.Take<Vector3>(width * height, out var vertices);
            vertexList = new List<Vector3>();

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    vertices[width * j + i] = deformer(i, j);
                }
            }

            var quadWidth = width - 1;
            var quadHeight = height - 1;
            var triangleCount = quadWidth * quadHeight * 2;
            pool.Take<Triangle>(triangleCount, out var triangles);

            for (int i = 0; i < quadWidth; ++i)
            {
                for (int j = 0; j < quadHeight; ++j)
                {
                    var triangleIndex = (j * quadWidth + i) * 2;
                    ref var triangle0 = ref triangles[triangleIndex];
                    ref var v00 = ref vertices[width * j + i];
                    ref var v01 = ref vertices[width * j + i + 1];
                    ref var v10 = ref vertices[width * (j + 1) + i];
                    ref var v11 = ref vertices[width * (j + 1) + i + 1];
                    triangle0.A = v00;
                    triangle0.B = v01;
                    triangle0.C = v10;
                    ref var triangle1 = ref triangles[triangleIndex + 1];
                    triangle1.A = v01;
                    triangle1.B = v11;
                    triangle1.C = v10;

                    vertexList.Add(triangle0.A);
                    vertexList.Add(triangle0.B);
                    vertexList.Add(triangle0.C);
                    vertexList.Add(triangle1.A);
                    vertexList.Add(triangle1.B);
                    vertexList.Add(triangle1.C);
                }
            }
            pool.Return(ref vertices);
            mesh = new BepuPhysics.Collidables.Mesh(triangles, scaling, pool);
        }
    }
}
