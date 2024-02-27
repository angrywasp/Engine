using Engine.Content;
using Engine.Interfaces;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Cameras;
using MonoGame.OpenGL;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using BepuPhysics;
using Engine.Physics;
using BepuPhysics.Collidables;
using Engine.Content.Model.Instance;
using System.Runtime.CompilerServices;
using AngryWasp.Helpers;
using System;
using System.Diagnostics;

namespace Engine.World.Objects
{
    //todo: need code to destroy static mesh when map unloaded
    [JsonObject(MemberSerialization.OptIn)]
    public class StaticMesh : IDrawableObjectDeferred, IDrawableObjectInstanced, IShadowCaster
    {
        public event EngineEventHandler<StaticMesh> Loaded;

        private static AsyncLock physicsLoadLock = new AsyncLock();

        private MeshInstance mesh;
        private EngineCore engine;
        private RasterizerState rs = RasterizerState.CullCounterClockwise;

        [JsonProperty] public string MeshPath { get; set; } = null;
        [JsonProperty] public string CollisionMeshPath { get; set; } = null;
        [JsonProperty] public bool AutoGenerateCollisionMesh { get; set; } = false;
        [JsonProperty] public WorldTransform3 GlobalTransform { get; set; } = new WorldTransform3();

        public MeshInstance Mesh => mesh;

        public async void LoadAsync(EngineCore engine)
        {
            try
            {
                this.engine = engine;
                mesh = await ContentLoader.LoadMeshAsync(engine, MeshPath).ConfigureAwait(false);

                engine.Scene.Graphics.AddInstancedDrawable(mesh.Key, this);

                var l = await physicsLoadLock.LockAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(CollisionMeshPath))
                {
                    var shape = ContentLoader.LoadCollisionMesh(engine, CollisionMeshPath);
                    var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);
                    var sd = new StaticDescription(GlobalTransform.Translation, GlobalTransform.Rotation, shapeIndex);
                    var staticHandle = engine.Scene.Physics.AddStaticBody(sd, new Material(30, 1, 1, float.MaxValue).ToPhysicsMaterial());
                    var staticReference = engine.Scene.Physics.Simulation.Statics.GetStaticReference(staticHandle);
                }
                else if (AutoGenerateCollisionMesh)
                {
                    Vector3[] v = mesh.Template.Positions.Select(x => x.Position).ToArray();
                    int triCount = mesh.Template.Indices.Length / 3;
                    engine.Scene.Physics.DefaultBufferPool.Take<Triangle>(triCount, out var buffer);

                    for (int i = 0; i < triCount; i++)
                    {
                        buffer[i] = new Triangle(
                            mesh.Template.Positions[mesh.Template.Indices[i * 3 + 0]].Position,
                            mesh.Template.Positions[mesh.Template.Indices[i * 3 + 1]].Position,
                            mesh.Template.Positions[mesh.Template.Indices[i * 3 + 2]].Position
                        );
                    }

                    var shape = new Mesh(buffer, GlobalTransform.Scale, engine.Scene.Physics.DefaultBufferPool);
                    var shapeIndex = engine.Scene.Physics.Simulation.Shapes.Add(shape);
                    var sd = new StaticDescription(GlobalTransform.Translation, GlobalTransform.Rotation, shapeIndex);
                    var staticHandle = engine.Scene.Physics.AddStaticBody(sd, new Material(30, 1, 1, float.MaxValue).ToPhysicsMaterial());
                    var staticReference = engine.Scene.Physics.Simulation.Statics.GetStaticReference(staticHandle);
                }

                l.Dispose();

                Loaded?.Invoke(this);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Update(Camera camera, GameTime gameTime) => UpdateMaterial(GlobalTransform.Matrix, camera, gameTime);

        private void ApplyAndDraw(string program)
        {
            foreach (var subMesh in mesh.SubMeshes)
            {
                engine.GraphicsDevice.RasterizerState = subMesh.Material.DoubleSided ? RasterizerState.CullNone : rs;
                subMesh.Material.Apply(program);
                engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, subMesh.BaseVertex, subMesh.StartIndex, subMesh.IndexCount);
            }
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Deferred;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldDraw(Camera camera) => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer)
        {
            foreach (var sm in mesh.SubMeshes)
                sm.Material.SetBuffers(gBuffer, lBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateMaterial(Matrix4x4 transform, Camera camera, GameTime gameTime)
        {
            foreach (var subMesh in mesh.SubMeshes)
                subMesh.Material.UpdateTransform(GlobalTransform.Matrix, camera.View, camera.Projection);
        }

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullClockwise;
            foreach (var sm in mesh.SubMeshes)
                sm.Material.UpdateTransform(GlobalTransform.Matrix, camera.View, camera.Projection);
        }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullCounterClockwise;
            foreach (var sm in mesh.SubMeshes)
                sm.Material.UpdateTransform(GlobalTransform.Matrix, camera.View, camera.Projection);
        }

        #endregion

        #region IDrawableObjectDeferred implementation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderToGBuffer(Camera camera) => ApplyAndDraw("RenderToGBuffer");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReconstructShading(Camera camera) => ApplyAndDraw("Reconstruct");

        #endregion

        #region IShadowCaster implementation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(BoundingFrustum frustum) => mesh.Intersects(frustum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(BoundingSphere sphere) => mesh.Intersects(sphere);

        public void RenderShadowMap(Matrix4x4 lightView, Matrix4x4 lightProjection)
        {
            foreach (var sm in mesh.SubMeshes)
            {
                engine.GraphicsDevice.RasterizerState = sm.Material.ShadowRasterizerState;
                sm.Material.UpdateTransform(GlobalTransform.Matrix, lightView, lightProjection);
                sm.Material.Apply("ShadowMap");
                engine.GraphicsDevice.DrawIndexedPrimitives(GLPrimitiveType.Triangles, sm.BaseVertex, sm.StartIndex, sm.IndexCount);
            }
        }

        #endregion
    }
}
