using System;
using System.Numerics;
using Engine.Content.Model.Template;

namespace Engine.Content.Model.Instance
{
    public class SkinInstance
    {
        private SkinTemplate template;
        private Matrix4x4[] skinTransforms;

        public Matrix4x4[] SkinTransforms => skinTransforms;

        public SkinInstance(SkinTemplate template)
        {
            this.template = template;
        }

        public void Update(SkeletonInstance skeleton) =>
            Update(template.JointsNodeIndices.Length, idx => template.JointsBindMatrices[idx], idx => skeleton.GetModelMatrix(template.JointsNodeIndices[idx]));

        public void Update(int count, Func<int, Matrix4x4> invBindMatrix, Func<int, Matrix4x4> currWorldMatrix)
        {
            if (skinTransforms == null || skinTransforms.Length != count)
                skinTransforms = new Matrix4x4[count];

            for (int i = 0; i < skinTransforms.Length; ++i)
                skinTransforms[i] = invBindMatrix(i) * currWorldMatrix(i);
        }
    }
}
