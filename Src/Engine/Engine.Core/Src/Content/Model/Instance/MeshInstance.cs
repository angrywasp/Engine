using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Engine.Content.Model.Template;
using Engine.Graphics.Materials;
using Microsoft.Xna.Framework;

namespace Engine.Content.Model.Instance
{
    //todo: move skinned mesh bounding box calculation to MeshProcessor
    public class SubMeshInstance
    {
        private SubMeshTemplate template;
        private MeshMaterial material;
        private SkinInstance skin;
        private Dictionary<int, BoundingBox> templateBoneBoundingBoxes;
        private Dictionary<int, BoundingBox> boneBoundingBoxes;
        private BoundingBox boundingBox;
        private int[] subMeshBoneIndices;

        public SubMeshTemplate Template => template;
        public MeshMaterial Material => material;
        public SkinInstance Skin => skin;
        public Dictionary<int, BoundingBox> BoneBoundingBoxes => boneBoundingBoxes;
        public BoundingBox BoundingBox => boundingBox;

        public int BaseVertex => template.BaseVertex;
        public int StartIndex => template.StartIndex;
        public int IndexCount => template.IndexCount;
        
        public SubMeshInstance(MeshTemplate mesh, SubMeshTemplate template)
        {
            this.template = template;
            this.boundingBox = template.BoundingBox;
            if (template.Skin != null)
            {
                this.skin = new SkinInstance(template.Skin);
                
                //the point of this is to get a list of bone indices that influence this submesh
                //then for each of those indices, we get the vertices that the individual bone influences
                //then we have a list of each bone and the vertices influenced by that bone
                //then we create a bounding box for each bone, encompassing those vertices
                //then when we apply the skin transforms, we can apply the bone transform to the bone bounding box
                //then we create a bounding box for the sub mesh by merging the bone bounding boxes
                //then the mesh gets a bounding box by mergung the submesh bounding boxes
                var l = new List<int>();
                var positions = mesh.Positions.AsSpan().Slice(template.BaseVertex, template.NumVertices).ToArray();
                var skinWeights = mesh.SkinWeights.AsSpan().Slice(template.BaseVertex, template.NumVertices).ToArray();
                foreach (var i in skinWeights)
                {
                    if (i.Weights.X > 0)
                        l.Add((int)i.Indices.X);

                    if (i.Weights.Y > 0)
                        l.Add((int)i.Indices.Y);

                    if (i.Weights.Z > 0)
                        l.Add((int)i.Indices.Z);

                    if (i.Weights.W > 0)
                        l.Add((int)i.Indices.W);
                }

                subMeshBoneIndices = l.Distinct().ToArray();
                templateBoneBoundingBoxes = new Dictionary<int, BoundingBox>();
                boneBoundingBoxes = new Dictionary<int, BoundingBox>();

                foreach (var boneIndex in subMeshBoneIndices)
                {
                    List<Vector3> points = new List<Vector3>();
                    for (int x = 0; x < skinWeights.Length; x++)
                    {
                        var s = skinWeights[x];
                        if (s.Indices.X == boneIndex || s.Indices.Y == boneIndex || s.Indices.Z == boneIndex || s.Indices.W == boneIndex)
                            points.Add(positions[x].Position);
                    }

                    templateBoneBoundingBoxes[boneIndex] = BoundingBox.CreateFromPoints(points);
                }

                boneBoundingBoxes = templateBoneBoundingBoxes.ToDictionary(k => k.Key, v => new BoundingBox());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(SkeletonInstance skeleton)
        {
            this.skin.Update(skeleton);
            this.material.SetBones(this.skin.SkinTransforms);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransformBoundingBox(Matrix4x4 transform)
        {
            if (skin != null && skin.SkinTransforms != null)
            {
                foreach (var b in templateBoneBoundingBoxes)
                    this.boneBoundingBoxes[b.Key] = BoundingBox.Transform(b.Value, skin.SkinTransforms[b.Key] * transform);

                var l = this.boneBoundingBoxes.Values.ToList();
                this.boundingBox = l[0];
                for (int i = 1; i < l.Count; i++)
                    this.boundingBox = BoundingBox.CreateMerged(this.boundingBox, l[i]);
            }
            else
                this.boundingBox = BoundingBox.Transform(template.BoundingBox, transform);
        }

        public async Task LoadAsync(EngineCore engine, bool skinned)
        {
            try
            {
                Log.Instance.Write("Loading submesh");
                this.material = ContentLoader.LoadMaterial(template.Material, skinned);
                await this.material.LoadAsync(engine).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }
        
        public async Task SetMaterialAsync(EngineCore engine, string materialPath, bool skinned)
        {
            this.material = ContentLoader.LoadMaterial(materialPath, skinned);
            await this.material.LoadAsync(engine).ConfigureAwait(false);
        }

        public void SetMaterial(MeshMaterial material) => this.material = material;
    }

    public class MeshInstance
    {
        private readonly MeshTemplate template;
        private readonly SubMeshInstance[] subMeshes;
        private readonly SharedMeshData sharedData;
        private readonly SkeletonInstance skeleton;
        private BoundingBox boundingBox;
        private readonly bool isSkinned;
        private readonly int key;
        private string selectedAnimation;
        private int selectedAnimationIndex = 0;

        public MeshTemplate Template => template;
        public SubMeshInstance[] SubMeshes => subMeshes;
        public SharedMeshData SharedData => sharedData;
        public SkeletonInstance Skeleton => skeleton;
        public BoundingBox BoundingBox => boundingBox;
        public string SelectedAnimation => selectedAnimation;
        public bool IsSkinned => isSkinned;
        public int Key => key;

        public MeshInstance(int key, MeshTemplate template, SharedMeshData sharedData)
        {
            this.key = key;
            this.template = template;
            subMeshes = new SubMeshInstance[template.SubMeshes.Length];
            for (int i = 0; i < subMeshes.Length; i++)
                subMeshes[i] = new SubMeshInstance(template, template.SubMeshes[i]);

            this.sharedData = sharedData;

            if (template.Type == Mesh_Type.Skinned)
            {
                isSkinned = true;
                this.skeleton = new SkeletonInstance(template.Skeleton);
                this.selectedAnimation = this.skeleton.AnimationTracks[selectedAnimationIndex].Name;
            }
        }

        public async Task LoadAsync(EngineCore engine)
        {
            foreach (var s in subMeshes)
                await s.LoadAsync(engine, isSkinned).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersects(BoundingFrustum frustum) => boundingBox.Intersects(frustum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Intersects(BoundingSphere sphere) => boundingBox.Intersects(sphere);

        public async Task SetMaterialAsync(EngineCore engine, string materialPath, int index)
        {
            if (index >= this.subMeshes.Length)
                return;

            await subMeshes[index].SetMaterialAsync(engine, materialPath, isSkinned).ConfigureAwait(false);
        }

        public void SetMaterial(MeshMaterial material, int index)
        {
            if (index >= this.subMeshes.Length)
                return;

            subMeshes[index].SetMaterial(material);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSelectedAnimation(string animation)
        {
            if (!isSkinned)
                return;

            this.selectedAnimation = animation;
            this.selectedAnimationIndex = skeleton.IndexOfTrack(SelectedAnimation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSkin(GameTime gameTime)
        {
            skeleton.SetAnimationFrame(selectedAnimationIndex, (float)gameTime.TotalGameTime.TotalSeconds);

            foreach (var s in subMeshes)
                s.Update(skeleton);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateBoundingBox()
        {
            this.boundingBox = subMeshes[0].BoundingBox;
            for (int i = 1; i < subMeshes.Length; i++)
                this.boundingBox = BoundingBox.CreateMerged(this.boundingBox, subMeshes[i].BoundingBox);
        }
    }
}