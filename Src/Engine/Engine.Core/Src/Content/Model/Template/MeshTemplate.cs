using System;
using System.Runtime.CompilerServices;
using Engine.Graphics.Vertices;
using Microsoft.Xna.Framework;

namespace Engine.Content.Model.Template
{
    public enum Mesh_Type
    {
        Default,

        Skinned
    }

    public class SubMeshTemplate
    {
        private readonly int baseVertex;
        private readonly int numVertices;
        private readonly int startIndex;
        private readonly int indexCount;
        private readonly BoundingBox boundingBox;
        private readonly SkinTemplate skin;
        private string material = "Engine/Materials/Default.material";

        public int BaseVertex => baseVertex;
        public int NumVertices => numVertices;
        public int StartIndex => startIndex;
        public int IndexCount => indexCount;
        public BoundingBox BoundingBox => boundingBox;
        public SkinTemplate Skin => skin;
        public string Material => material;

        public SubMeshTemplate(int baseVertex, int numVertices, int startIndex, int indexCount, BoundingBox boundingBox, SkinTemplate skin, string material)
        {
            this.baseVertex = baseVertex;
            this.numVertices = numVertices;
            this.startIndex = startIndex;
            this.indexCount = indexCount;
            this.boundingBox = boundingBox;
            this.skin = skin;
            if (!string.IsNullOrEmpty(material))
                this.material = material;
        }

        //todo: remove and make material field readonly
        [Obsolete]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseMaterial(string material) => this.material = material;
    }

    public class MeshTemplate
    {
        private readonly Mesh_Type type;
        private readonly VertexPositionTexture[] positions;
        private readonly VertexNormalTangentBinormal[] normals;
        private readonly VertexSkinWeight[] skinWeights;
        private readonly int[] indices;
        private readonly SubMeshTemplate[] subMeshes;
        private BoundingBox boundingBox;
        private SkeletonTemplate skeleton;

        public Mesh_Type Type => type;
        public VertexPositionTexture[] Positions => positions;
        public VertexNormalTangentBinormal[] Normals => normals;
        public VertexSkinWeight[] SkinWeights => skinWeights;
        public int[] Indices => indices;
        public SubMeshTemplate[] SubMeshes => subMeshes;
        public BoundingBox BoundingBox => boundingBox;
        public SkeletonTemplate Skeleton => skeleton;

        public MeshTemplate(int[] indices, VertexPositionTexture[] positions, VertexNormalTangentBinormal[] normals, VertexSkinWeight[] skinWeights, BoundingBox boundingBox, SubMeshTemplate[] subMeshes, SkeletonTemplate skeleton)
        {
            this.type = (skinWeights != null && skinWeights.Length > 0) ? Mesh_Type.Skinned : Mesh_Type.Default;
            this.indices = indices;
            this.positions = positions;
            this.normals = normals;
            this.skinWeights = skinWeights;
            this.boundingBox = boundingBox;
            this.subMeshes = subMeshes;
            this.skeleton = skeleton;
        }
    }
}