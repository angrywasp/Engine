using System;
using System.Numerics;
using Engine.Content.Model.Template;

namespace Engine.Content.Model.Instance
{
    public class SkeletonNodeInstance
    {
        private SkeletonNodeTemplate template;
        private SkeletonNodeInstance parent;
        private Matrix4x4 localMatrix;
        private Matrix4x4? modelMatrix;

        public String Name => template.Name;
        
        public SkeletonNodeInstance VisualParent => parent;

        public Matrix4x4 LocalMatrix
        {
            get => localMatrix;
            set
            {
                localMatrix = value;
                modelMatrix = null;
            }
        }

        public Matrix4x4 ModelMatrix
        {
            get => GetModelMatrix();
            set => SetModelMatrix(value);
        }
        
        private bool _TransformChainIsDirty
        {
            get
            {
                if (!modelMatrix.HasValue)
                    return true;

                return parent == null ? false : parent._TransformChainIsDirty;
            }
        }

        internal SkeletonNodeInstance(SkeletonNodeTemplate template, SkeletonNodeInstance parent)
        {
            this.template = template;
            this.parent = parent;
        }

        private Matrix4x4 GetModelMatrix()
        {
            if (!_TransformChainIsDirty) return modelMatrix.Value;

            modelMatrix = parent == null ? localMatrix : Matrix4x4.Multiply(localMatrix, parent.ModelMatrix);

            return modelMatrix.Value;
        }

        private void SetModelMatrix(Matrix4x4 xform)
        {
            if (parent == null) { LocalMatrix = xform; return; }

            var pxform = parent.GetModelMatrix();
            Matrix4x4.Invert(pxform, out Matrix4x4 ipwm);

            LocalMatrix = Matrix4x4.Multiply(xform, ipwm);
        }

        public void SetPoseTransform() => SetAnimationFrame(-1, 0);
        
        public void SetAnimationFrame(int trackIndex, float time) => this.LocalMatrix = template.GetLocalMatrix(trackIndex, time);

        public void SetAnimationFrame(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight) => this.LocalMatrix = template.GetLocalMatrix(track, time, weight);

        public override string ToString() => template.Name;
    }
}
