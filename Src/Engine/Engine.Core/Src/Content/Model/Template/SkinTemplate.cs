using System.Numerics;

namespace Engine.Content.Model.Template
{
    public class SkinTemplate
    {
        private string name;
        private int[] jointsNodeIndices;
        private Matrix4x4[] jointsBindMatrices;

        public string Name => name;
        public int[] JointsNodeIndices => jointsNodeIndices;
        public Matrix4x4[] JointsBindMatrices => jointsBindMatrices;

        public SkinTemplate(string name, (SkeletonNodeTemplate, Matrix4x4)[] skinNodes)
        {
            this.name = name;
            this.jointsNodeIndices = new int[skinNodes.Length];
            this.jointsBindMatrices = new Matrix4x4[skinNodes.Length];

            for (int i = 0; i < this.jointsNodeIndices.Length; ++i)
            {
                var (j, ibm) = skinNodes[i];

                this.jointsNodeIndices[i] = j.ThisIndex;
                this.jointsBindMatrices[i] = ibm;
            }
        }

        public SkinTemplate(string name, int[] jointsNodeIndices, Matrix4x4[] jointsBindMatrices)
        {
            this.name = name;
            this.jointsNodeIndices = jointsNodeIndices;
            this.jointsBindMatrices = jointsBindMatrices;
        }
    }
}
