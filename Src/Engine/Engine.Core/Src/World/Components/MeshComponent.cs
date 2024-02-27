using Engine.Content;
using Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Cameras;
using MonoGame.OpenGL;
using Engine.Scene;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Engine.Content.Model.Instance;
using System.Runtime.CompilerServices;
using Engine.Debug.Shapes;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Engine.World.Components
{
    public class MeshComponentType : ComponentType
    {
        public override string ComponentClass => "Engine.World.Components.MeshComponent";

        [JsonProperty] public string Mesh { get; set; }
        [JsonProperty] public bool Instance { get; set; } = true;

        public override string ToString() => $"Mesh: {(string.IsNullOrEmpty(Mesh) ? "null" : Mesh)}";
    }

    public class MeshComponent : Component, IDrawableObjectDeferred, IDrawableObjectInstanced, IShadowCaster
    {
        private MeshComponentType _type = null;

        public new MeshComponentType Type => _type;

        private MeshInstance mesh;
        private RasterizerState rs = RasterizerState.CullCounterClockwise;
        private SceneGraphics graphics;
        private bool instanced;
        //private DebugBoundingBox[] subMeshDebugBoundingBoxes;
        //private Dictionary<int, DebugBoundingBox>[] subMeshBoneDebugBoundingBoxes;
        private DebugBoundingBox meshDebugBoundingBox;

        public MeshInstance Mesh => mesh;

        public Matrix4x4 LastSentTransform { get; set; } = Matrix4x4.Identity;

        public override void Update(Camera camera, GameTime gameTime)
        {
            base.Update(camera, gameTime);

            meshDebugBoundingBox.Update(mesh.BoundingBox);
            /*for (int i = 0; i < mesh.SubMeshes.Length; i++)
            {
                if (mesh.IsSkinned)
                {
                    foreach (var b in subMeshBoneDebugBoundingBoxes[i])
                        subMeshBoneDebugBoundingBoxes[i][b.Key].Update(mesh.SubMeshes[i].BoneBoundingBoxes[b.Key]);
                }

                subMeshDebugBoundingBoxes[i].Update(mesh.SubMeshes[i].BoundingBox);
            }*/

            if (mesh.IsSkinned)
            {
                mesh.UpdateSkin(gameTime);
                RecalculateBoundingBox();
            }
        }

        private void ApplyAndDraw(string program)
        {
            foreach (var subMesh in mesh.SubMeshes)
            {
                graphics.Device.RasterizerState = subMesh.Material.DoubleSided ? RasterizerState.CullNone : rs;
                subMesh.Material.Apply(program);
                graphics.Device.DrawIndexedPrimitives(GLPrimitiveType.Triangles, subMesh.BaseVertex, subMesh.StartIndex, subMesh.IndexCount);
            }
        }

        public override async Task CreateFromTypeAsync(GameObject parent)
        {
            try
            {
                this.graphics = parent.engine.Scene.Graphics;

                this.instanced = Type.Instance;

                if (!string.IsNullOrEmpty(_type.Mesh))
                    mesh = await ContentLoader.LoadMeshAsync(parent.engine, _type.Mesh).ConfigureAwait(false);

                meshDebugBoundingBox = graphics.DebugRenderer.QueueShapeAdd(mesh.BoundingBox, Color.Yellow);
                /*subMeshDebugBoundingBoxes = new DebugBoundingBox[mesh.SubMeshes.Length];

                subMeshBoneDebugBoundingBoxes = new Dictionary<int, DebugBoundingBox>[mesh.SubMeshes.Length];
                for (int i = 0; i < mesh.SubMeshes.Length; i++)
                {
                    if (mesh.IsSkinned)
                    {
                        subMeshBoneDebugBoundingBoxes[i] = new Dictionary<int, DebugBoundingBox>();

                        foreach (var b in mesh.SubMeshes[i].BoneBoundingBoxes)
                            subMeshBoneDebugBoundingBoxes[i].Add(b.Key, graphics.DebugRenderer.QueueShapeAdd(b.Value, Color.Fuchsia));
                    }

                    subMeshDebugBoundingBoxes[i] = graphics.DebugRenderer.QueueShapeAdd(mesh.SubMeshes[i].BoundingBox, Color.Green);
                }*/

                await base.CreateFromTypeAsync(parent).ConfigureAwait(false);

                RecalculateBoundingBox();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        public override void OnAddedToMap()
        {
            base.OnAddedToMap();

            if (mesh.IsSkinned || !instanced)
                graphics.AddDrawable(this.Mesh.Key, this.Mesh.SharedData, this);
            else
                graphics.AddInstancedDrawable(this.Mesh.Key, this);
        }

        public override void OnRemovedFromMap()
        {
            base.OnRemovedFromMap();
            graphics.DebugRenderer.QueueShapeRemove(meshDebugBoundingBox);

            /*if (subMeshDebugBoundingBoxes != null)
                foreach (var s in subMeshDebugBoundingBoxes)
                    graphics.DebugRenderer.QueueShapeRemove(s);

            if (subMeshBoneDebugBoundingBoxes != null)
                foreach (var b in subMeshBoneDebugBoundingBoxes)
                    foreach (var bb in b.Values)
                        graphics.DebugRenderer.QueueShapeRemove(bb);*/

            if (instanced)
                graphics.RemoveInstancedDrawable(this.Mesh.Key, this);
            else
                graphics.RemoveDrawable(this.Mesh.Key, this);
        }

        protected override void OnGlobalTransformChanged(WorldTransform3 globalTransform)
        {
            base.OnGlobalTransformChanged(globalTransform);
            RecalculateBoundingBox();
        }

        private void RecalculateBoundingBox()
        {
            foreach (var s in mesh.SubMeshes)
                s.TransformBoundingBox(globalTransform.Matrix);

            mesh.UpdateBoundingBox();
        }

        #region IDrawableObject implementation

        public Render_Group RenderGroup => Render_Group.Deferred;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldDraw(Camera camera) => mesh.Intersects(camera.Frustum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBuffers(LBuffer lBuffer, GBuffer gBuffer)
        {
            foreach (var sm in mesh.SubMeshes)
                sm.Material.SetBuffers(gBuffer, lBuffer);
        }

        public void PreDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullClockwise;
            foreach (var sm in mesh.SubMeshes)
            {
                sm.Material.Clipping.Enable(plane.W);
                sm.Material.UpdateTransform(globalTransform.Matrix, camera.View, camera.Projection);
            }
        }

        public void PostDrawReflection(Camera camera, Matrix4x4 matrix, Vector4 plane)
        {
            rs = RasterizerState.CullCounterClockwise;
            foreach (var sm in mesh.SubMeshes)
            {
                sm.Material.Clipping.Disable();
                sm.Material.UpdateTransform(globalTransform.Matrix, camera.View, camera.Projection);
            }
        }

        #endregion

        #region IDrawableObjectDeferred implementation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderToGBuffer(Camera camera)
        {
            foreach (var subMesh in mesh.SubMeshes)
                subMesh.Material.UpdateTransform(globalTransform.Matrix, camera.View, camera.Projection);

            ApplyAndDraw("RenderToGBuffer");
        }

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
                graphics.Device.RasterizerState = sm.Material.ShadowRasterizerState;
                sm.Material.UpdateTransform(globalTransform.Matrix, lightView, lightProjection);
                sm.Material.Apply("ShadowMap");
                graphics.Device.DrawIndexedPrimitives(GLPrimitiveType.Triangles, sm.BaseVertex, sm.StartIndex, sm.IndexCount);
            }
        }

        #endregion
    }
}
