using Engine.Content;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Engine.Cameras;
using Engine.Configuration;
using System.Numerics;
using Engine.Debug;
using AngryWasp.Logger;
using Engine.Graphics.Effects;
using Engine.Graphics.Effects.Lights;
using Engine.World.Components.Lights;
using Engine.Content.Model;
using Engine.Content.Model.Instance;
using Engine.Helpers;
using Engine.AssetTransport;
using System.IO;
using System.Diagnostics;
using Engine.World.Objects;
using Engine.Graphics.Vertices;
using System;

namespace Engine.Scene
{
    public enum Render_Group
    {
        Deferred,
        Forward
    }

    public class SceneGraphics
    {
        private GBuffer gBuffer;
        private LBuffer lBuffer;
        private RenderTarget2D outputTexture;

        private EngineCore engine;
        private GraphicsDevice graphicsDevice;
        private QuadRenderer quad;

        private ClearBufferEffect clearBufferEffect;
        private ReconstructDepthEffect reconstructDepthEffect;
        private AmbientLightEffect ambientLightEffect;
        private ReconstructEffect reconstructEffect;
        public MeshInstance lightMesh;

        private DirectionalShadowRenderer directionalShadowRenderer;
        private SpotShadowRenderer spotShadowRenderer;

        private List<PointLightComponent> allPointLights = new List<PointLightComponent>();
        private List<SpotLightComponent> allSpotLights = new List<SpotLightComponent>();

        private List<SpotLightEntry> spotLightEntries;
        private List<PointLightComponent> visiblePointLights = new List<PointLightComponent>();
        private List<SpotLightEntry> spotLightShadowCasters = new List<SpotLightEntry>();

        private List<DirectionalLightEntry> directionalLightEntries = new List<DirectionalLightEntry>();
        private List<DirectionalLightEntry> directionalLightShadowCasters = new List<DirectionalLightEntry>();

        private DynamicInstancingGroupManager dynamicInstancingGroupManager;

        public GBuffer GBuffer => gBuffer;

        public LBuffer LBuffer => lBuffer;

        public GraphicsDevice Device => graphicsDevice;

        public QuadRenderer Quad => quad;

        public List<DirectionalLightEntry> DirectionalLightShadowCasters => directionalLightShadowCasters;

        private List<IDrawableObject> allDrawableObjects = new List<IDrawableObject>();
        private Dictionary<int, (SharedMeshData SharedMeshData, List<IDrawableObjectDeferred> MeshList)> allDeferredObjects =
            new Dictionary<int, (SharedMeshData SharedMeshData, List<IDrawableObjectDeferred> MeshList)>();
        private List<IDrawableObjectForward> allForwardObjects = new List<IDrawableObjectForward>();
        private Dictionary<int, (SharedMeshData SharedMeshData, List<IDrawableObjectDeferred> MeshList)> visibleDeferredObjects =
            new Dictionary<int, (SharedMeshData SharedMeshData, List<IDrawableObjectDeferred> MeshList)>();
        private List<IDrawableObjectForward> visibleForwardObjects = new List<IDrawableObjectForward>();

        private List<ParticleEntry> particleEntries = new List<ParticleEntry>();
        private List<ParticleSystem> allParticleSystems = new List<ParticleSystem>();
        private List<ParticleSystem> visibleParticleSystems = new List<ParticleSystem>();

        public DynamicInstancingGroupManager DynamicInstancingGroupManager => dynamicInstancingGroupManager;

        private DepthStencilState depthStateReconstructZ = new DepthStencilState()
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.Always
        };

        private DepthStencilState depthStateDrawLights = new DepthStencilState()
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.GreaterEqual
        };

        private DepthStencilState lightCWDepthState = new DepthStencilState()
        {
            DepthBufferEnable = false,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.Greater
        };

        public RenderTarget2D OutputTexture => outputTexture;

        public bool DebugDraw { get; set; } = true;

        public DirectionalShadowRenderer DirectionalShadowRenderer => directionalShadowRenderer;
        public SpotShadowRenderer SpotShadowRenderer => spotShadowRenderer;

        public DebugRenderer DebugRenderer { get; set; }

        public AmbientLightEffect AmbientLightEffect => ambientLightEffect;

        public SceneGraphics(EngineCore engine)
        {
            this.engine = engine;
            graphicsDevice = engine.GraphicsDevice;
            DebugRenderer = new DebugRenderer(graphicsDevice);

            quad = new QuadRenderer(graphicsDevice, Settings.Engine.Resolution);

            CreateRenderTargets(Settings.Engine.Resolution.X, Settings.Engine.Resolution.Y);

            directionalShadowRenderer = new DirectionalShadowRenderer(engine);
            spotShadowRenderer = new SpotShadowRenderer(engine);

            clearBufferEffect = new ClearBufferEffect(engine.GraphicsDevice);
            clearBufferEffect.Load();

            reconstructDepthEffect = new ReconstructDepthEffect(engine.GraphicsDevice);
            reconstructDepthEffect.Load();

            reconstructEffect = new ReconstructEffect(engine.GraphicsDevice);
            reconstructEffect.Load();

            ambientLightEffect = new AmbientLightEffect(engine.GraphicsDevice);
            ambientLightEffect.Load();

            //bypass the ContentLoader cause we don't need the async loading that comes after the file reading
            var lightMeshTemplate = MeshReader.Read(File.ReadAllBytes(EngineFolders.ContentPathVirtualToReal("Engine/Renderer/Meshes/LightMeshes/LightMesh.mesh")));
            var buffers = new SharedMeshData(graphicsDevice, lightMeshTemplate.Positions, lightMeshTemplate.Normals, lightMeshTemplate.Indices);
            lightMesh = new MeshInstance(0, lightMeshTemplate, buffers);

            dynamicInstancingGroupManager = new DynamicInstancingGroupManager(engine);
        }

        public void CreateRenderTargets(int w, int h)
        {
            outputTexture = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Rgba64, DepthFormat.Depth32F, 0, RenderTargetUsage.PreserveContents);

            gBuffer = new GBuffer(graphicsDevice, w, h);
            lBuffer = new LBuffer(graphicsDevice, w, h);

            quad.UpdateHalfPixelOffset(new Vector2(w, h));
        }

        public void Update(Camera camera, GameTime gameTime)
        {
            UpdateVisibleLights(camera);
            UpdateVisibleObjects(camera);
            UpdateVisibleParticleSystems(camera);

            SelectShadowCasters();

            foreach (var ig in dynamicInstancingGroupManager.Groups.Values)
                ig.Update(camera);

            if (DebugDraw)
                DebugRenderer.Update();
        }

        private void SelectShadowCasters()
        {
            spotShadowRenderer.Clear();
            spotLightShadowCasters.Clear();

            for (int i = 0; i < spotLightEntries.Count; i++)
            {
                var l = spotLightEntries[i];

                l.ShadowMap = spotShadowRenderer.GetFreeSpotShadowMap();

                if (l.ShadowMap != null)
                    spotLightShadowCasters.Add(l);

                spotLightEntries[i] = l;
            }
        }

        private void GenerateShadows(Camera camera)
        {
            for (int i = 0; i < directionalLightShadowCasters.Count; i++)
                directionalShadowRenderer.GenerateShadowTexture(this, graphicsDevice, directionalLightShadowCasters[i], camera);

            for (int i = 0; i < spotLightShadowCasters.Count; i++)
                spotShadowRenderer.GenerateShadowTexture(this, graphicsDevice, spotLightShadowCasters[i]);
        }

        public void Draw(RenderTarget2D renderTarget, Camera camera)
        {
            RenderToGBuffer(camera);
            ConstructLighting(camera, RasterizerState.CullCounterClockwise);
            ConstructFinalImage(camera, renderTarget);
            DrawForwardPass(camera);
            GenerateShadows(camera);

            if (DebugDraw)
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                DebugRenderer.Draw(camera);
            }
        }

        public void DrawReflection(GBuffer g, LBuffer l, RenderTarget2D renderTarget, Camera camera)
        {
            RenderToGBuffer(camera);
            ConstructLighting(camera, RasterizerState.CullCounterClockwise);
            ConstructFinalImage(camera, renderTarget);
            DrawForwardPass(camera);
        }

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            //todo: we should be able to optimize this to only visible objects in all render groups
            foreach (IDrawableObject mesh in allDrawableObjects.ToArray())
                mesh.PreDrawReflection(camera, matrix, plane);
        }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            foreach (IDrawableObject mesh in allDrawableObjects.ToArray())
                mesh.PostDrawReflection(camera, matrix, plane);
        }

        private void ClearBuffer(RenderTargetBinding[] buffer, int pass)
        {
            graphicsDevice.SetRenderTargets(buffer);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            clearBufferEffect.Apply(pass);
            quad.RenderQuad();

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void RenderToGBuffer(Camera camera)
        {
            ClearBuffer(gBuffer.Binding, 0);
            foreach (var list in visibleDeferredObjects.Values)
            {
                graphicsDevice.SetVertexBuffers(list.SharedMeshData.RenderToGBufferBinding);
                graphicsDevice.SetIndexBuffer(list.SharedMeshData.IndexBuffer);

                foreach (var obj in list.MeshList)
                    obj.RenderToGBuffer(camera);
            }

            foreach (var ig in dynamicInstancingGroupManager.Groups.Values)
                ig.RenderToGBuffer();
        }

        public void ReconstructDepthBuffer(Camera camera)
        {
            reconstructDepthEffect.FarClip = camera.FarClip;
            reconstructDepthEffect.ProjectionValues = new Vector2(camera.Projection.M33, camera.Projection.M43);
            reconstructDepthEffect.GDepthBuffer = gBuffer.Depth;
            reconstructDepthEffect.Apply();

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = depthStateReconstructZ;

            quad.RenderQuad();
        }

        private void ConstructLighting(Camera camera, RasterizerState rs)
        {
            ClearBuffer(lBuffer.Binding, 1);
            //graphicsDevice.SetRenderTargets(lBuffer.Binding);
            //graphicsDevice.Clear(Vector4.Zero);
            ReconstructDepthBuffer(camera);

            graphicsDevice.BlendState = BlendState.Additive;

            graphicsDevice.DepthStencilState = depthStateDrawLights;
            graphicsDevice.BlendState = BlendState.Opaque;

            ambientLightEffect.Update(gBuffer, camera.Position);
            ambientLightEffect.DefaultProgram.Apply();
            quad.RenderQuad();

            graphicsDevice.BlendState = BlendState.Additive;

            //draw directional lights 
            foreach (var le in directionalLightEntries)
            {
                le.Light.SetGBuffer(gBuffer);
                le.Light.Draw();
            }

            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            graphicsDevice.DepthStencilState = lightCWDepthState;

            if (spotLightEntries.Count > 0 || visiblePointLights.Count > 0)
            {
                graphicsDevice.SetVertexBuffer(lightMesh.SharedData.PositionBuffer);
                graphicsDevice.SetIndexBuffer(lightMesh.SharedData.IndexBuffer);

                foreach (var le in spotLightEntries)
                {
                    le.Light.SetGBuffer(gBuffer);
                    le.Light.Draw(lightMesh);
                }

                foreach (var le in visiblePointLights)
                {
                    le.SetGBuffer(gBuffer);
                    le.Draw(lightMesh);
                }
            }

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        /*private void ConstructFinalImage(Camera camera, RenderTarget2D rt)
        {
            graphicsDevice.SetRenderTarget(rt);
            graphicsDevice.Clear(Vector4.Zero);

            reconstructEffect.FarClip = camera.FarClip;
            reconstructEffect.GBufferPixelSize = new Vector2(0.5f / rt.Width, 0.5f / rt.Height);
            reconstructEffect.ProjectionValues = new Vector2(camera.Projection.M33, camera.Projection.M43);
            reconstructEffect.GDepthBuffer = gBuffer.Depth;
            reconstructEffect.LRadianceBuffer = lBuffer.Radiance;
            reconstructEffect.LAmbientBuffer = lBuffer.Ambient;

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = depthStateReconstructZ;

            reconstructEffect.Apply();
            quad.RenderQuad();
        }*/

        private void ConstructFinalImage(Camera camera, RenderTarget2D rt)
        {
            //Draw the result
            graphicsDevice.SetRenderTarget(rt);
            graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Vector4.Zero, 1.0f, 0);

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Additive;

            foreach (var list in visibleDeferredObjects.Values)
            {
                graphicsDevice.SetVertexBuffers(list.SharedMeshData.ReconstructBinding);
                graphicsDevice.SetIndexBuffer(list.SharedMeshData.IndexBuffer);

                foreach (var obj in list.MeshList)
                {
                    obj.SetBuffers(lBuffer, gBuffer);
                    obj.ReconstructShading(camera);
                }
            }

            foreach (var ig in dynamicInstancingGroupManager.Groups.Values)
            {
                ig.SetBuffers(gBuffer, lBuffer);
                ig.Reconstruct();
            }

            //g/raphicsDevice.Clear(ClearOptions.Target, Vector4.Zero, 1.0f, 0);

            //reconstructEffect.FarClip = camera.FarClip;
            //reconstructEffect.GBufferPixelSize = new Vector2(1.0f / rt.Width, 1.0f / rt.Height);
            //reconstructEffect.ProjectionValues = new Vector2(camera.Projection.M33, camera.Projection.M43);
            //reconstructEffect.GDepthBuffer = gBuffer.Depth;
            reconstructEffect.LRadianceBuffer = lBuffer.Radiance;
            reconstructEffect.LAmbientBuffer = lBuffer.Ambient;

            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.BlendState = BlendState.Opaque;

            reconstructEffect.Apply();
            quad.RenderQuad();
        }

        private void DrawForwardPass(Camera camera)
        {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (IDrawableObjectForward go in visibleForwardObjects)
                go.RenderForwardPass(camera);

            engine.GraphicsDevice.BlendState = BlendState.Additive;
            //todo: if we reverse the winding of the particles, we can avoid a rasterizer state change here
            engine.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var pe in particleEntries)
                pe.ParticleSystem.Draw();
            graphicsDevice.BlendState = BlendState.Opaque;
        }

        private void UpdateVisibleLights(Camera camera)
        {
            visiblePointLights.Clear();

            spotLightEntries = allSpotLights
                .Where(x => x.ShouldDraw(camera))
                .Select(x => new SpotLightEntry { Light = x, Priority = 1000 * Vector3.Distance(x.GlobalTransform.Translation, camera.Position) })
                .OrderBy(x => x.Priority)
                .ToList();

            visiblePointLights = allPointLights.Where(x => x.ShouldDraw(camera)).ToList();

            /*foreach (var light in allSpotLights)
                if (light.ShouldDraw(camera))
                {
                    SpotLightEntry lightEntry = new SpotLightEntry();
                    lightEntry.Light = light;
                    lightEntry.Priority = 1000 * Vector3.Distance(light.GlobalTransform.Translation, camera.Position);
                    spotLightEntries.Add(lightEntry);
                }*/

            /*foreach (var light in allPointLights)
                if (light.ShouldDraw(camera))
                    visiblePointLights.Add(light);*/

            //spotLightEntries = spotLightEntries.OrderBy(x => x.Priority).ToList();
        }

        internal void UpdateVisibleObjects(Camera camera)
        {
            UpdateVisibleDeferredObjects(camera);
            UpdateVisibleForwardObjects(camera);
        }

        internal void UpdateVisibleParticleSystems(Camera camera)
        {
            particleEntries = allParticleSystems.Where(x => x.Enabled && camera.Frustum.Intersects(x.GlobalBoundingBox))
                .Select(x => new ParticleEntry { ParticleSystem = x, Priority = 1000 * Vector3.Distance(x.GlobalTransform.Translation, camera.Position) })
                .OrderBy(x => x.Priority)
                .ToList();
        }

        internal void UpdateVisibleDeferredObjects(Camera camera)
        {
            visibleDeferredObjects.Clear();
            foreach (var o in allDeferredObjects)
            {
                var l = new List<IDrawableObjectDeferred>();

                foreach (var obj in o.Value.MeshList)
                    if (obj.ShouldDraw(camera))
                        l.Add(obj);

                if (l.Count > 0)
                    visibleDeferredObjects.Add(o.Key, (o.Value.SharedMeshData, l));
            }
        }

        internal void UpdateVisibleForwardObjects(Camera camera)
        {
            visibleForwardObjects.Clear();
            foreach (var o in allForwardObjects.ToArray())
            {
                if (o.ShouldDraw(camera))
                    visibleForwardObjects.Add(o);
            }
        }

        internal void AddDirectionalLight(DirectionalLightComponent light)
        {
            Threading.BlockOnUIThread(() =>
            {
                DirectionalLightEntry lightEntry = new DirectionalLightEntry();
                lightEntry.Light = light;
                lightEntry.Priority = float.MinValue;

                lightEntry.ShadowMap = directionalShadowRenderer.GetDirectionalShadowMapEntry();
                lightEntry.ShadowMap.LightComponent = light;
                directionalLightEntries.Add(lightEntry);
                directionalLightShadowCasters.Add(lightEntry);

                Log.Instance.Write("Directional light added to scene");
            });
        }

        internal void RemoveDirectionalLight(DirectionalLightComponent light)
        {
            var i = directionalLightEntries.Where(x => x.Light == light).FirstOrDefault();
            directionalLightEntries.Remove(i);
            directionalLightShadowCasters.Remove(i);
            Log.Instance.Write("Directional light removed from scene");
        }

        internal void AddSpotLight(SpotLightComponent light)
        {
            allSpotLights.Add(light);
            Log.Instance.Write("Spot light added to scene");
        }

        internal void RemoveSpotLight(SpotLightComponent light)
        {
            var i = spotLightShadowCasters.Where(x => x.Light == light).FirstOrDefault();
            allSpotLights.Remove(light);
            spotLightShadowCasters.Remove(i);
            Log.Instance.Write("Spot light removed from scene");
        }

        internal void AddPointLight(PointLightComponent light)
        {
            allPointLights.Add(light);
            Log.Instance.Write("Point light added to scene");
        }

        internal void RemovePointLight(PointLightComponent light)
        {
            allPointLights.Remove(light);
            Log.Instance.Write("Point light added to scene");
        }

        internal void AddDrawable(int key, SharedMeshData meshData, IDrawableObject obj)
        {
            Threading.BlockOnUIThread(() =>
            {
                Log.Instance.Write("Drawable added to scene");
                allDrawableObjects.Add(obj);

                if (!allDeferredObjects.ContainsKey(key))
                    allDeferredObjects.Add(key, (meshData, new List<IDrawableObjectDeferred>()));

                switch (obj.RenderGroup)
                {
                    case Render_Group.Deferred:
                        allDeferredObjects[key].MeshList.Add((IDrawableObjectDeferred)obj);
                        break;
                    case Render_Group.Forward:
                        allForwardObjects.Add((IDrawableObjectForward)obj);
                        break;
                }
            });
        }

        internal void AddInstancedDrawable(int key, IDrawableObjectInstanced obj)
        {
            Threading.BlockOnUIThread(() =>
            {
                try
                {
                    Log.Instance.Write("Instanced mesh component added to scene");
                    dynamicInstancingGroupManager.Add(key, obj);
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteFatalException(ex);
                }
            });
        }

        internal void RemoveInstancedDrawable(int key, IDrawableObjectInstanced obj)
        {
            Threading.BlockOnUIThread(() =>
            {
                Log.Instance.Write("Instanced mesh component removed from scene");
                dynamicInstancingGroupManager.Remove(key, obj);
            });
        }

        internal void AddParticleSystem(ParticleSystem p)
        {
            Threading.BlockOnUIThread(() =>
            {
                Log.Instance.Write("Particle system added to scene");
                allParticleSystems.Add(p);
            });
        }

        public void RemoveDrawable(int key, IDrawableObject obj)
        {
            //todo: should probably remove the shared mesh data if the list is empty
            allDrawableObjects.Remove(obj);

            switch (obj.RenderGroup)
            {
                case Render_Group.Deferred:
                        allDeferredObjects[key].MeshList.Remove((IDrawableObjectDeferred)obj);
                        if (allDeferredObjects[key].MeshList.Count == 0)
                            allDeferredObjects.Remove(key);
                    break;
                case Render_Group.Forward:
                    allForwardObjects.Remove((IDrawableObjectForward)obj);
                    break;
            }
        }

        internal void RemoveParticleSystem(ParticleSystem p)
        {
            allParticleSystems.Remove(p);
        }

        public void DestroyRenderables()
        {
            allDrawableObjects.Clear();
            allDeferredObjects.Clear();
            allForwardObjects.Clear();
            allParticleSystems.Clear();
            visibleDeferredObjects.Clear();
            visibleForwardObjects.Clear();
        }

        public void Destroy()
        {
            DestroyRenderables();
            graphicsDevice.SetRenderTarget(null);

            graphicsDevice.BlendState.Dispose();
            graphicsDevice.DepthStencilState.Dispose();
            graphicsDevice.Dispose();
        }

        internal void GetShadowCasters(BoundingFrustum frustum, List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)> list)
        {
            foreach (var o in allDeferredObjects)
            {
                var l = o.Value.MeshList.Where(x => (x is IShadowCaster y && y.Intersects(frustum))).Select(x => (IShadowCaster)x).ToList();

                if (l.Count > 0)
                    list.Add((o.Value.SharedMeshData, l));
            }
        }

        internal void GetShadowCasters(BoundingSphere sphere, List<(SharedMeshData SharedMeshData, List<IShadowCaster> MeshList)> list)
        {
            foreach (var o in allDeferredObjects)
            {
                var l = o.Value.MeshList.Where(x => (x is IShadowCaster y && y.Intersects(sphere))).Select(x => (IShadowCaster)x).ToList();

                if (l.Count > 0)
                    list.Add((o.Value.SharedMeshData, l));
            }
        }
    }
}
