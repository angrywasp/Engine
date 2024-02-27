using System;
using System.Numerics;
using SharpGLTF.Transforms;

namespace Engine.Content.Model.Template
{
    public class SkeletonNodeTemplate
    {
        private readonly string name;
        private readonly int thisIndex;
        private readonly int parentIndex;
        private readonly int[] childIndices;
        private readonly Matrix4x4 localMatrix;
        private readonly bool useAnimatedTransforms;
        private readonly AnimatableProperty<Vector3> localScale;
        private readonly AnimatableProperty<Quaternion> localRotation;
        private readonly AnimatableProperty<Vector3> localTranslation;

        public string Name => name;
        public int ThisIndex => thisIndex;
        public int ParentIndex => parentIndex;
        public int[] ChildIndices => childIndices;
        public Matrix4x4 LocalMatrix => localMatrix;
        public AnimatableProperty<Vector3> LocalScale => localScale;
        public AnimatableProperty<Quaternion> LocalRotation => localRotation;
        public AnimatableProperty<Vector3> LocalTranslation => localTranslation;

        public SkeletonNodeTemplate(string name, int thisIndex, int parentIndex, int[] childIndices, Matrix4x4 localMatrix,
            AnimatableProperty<Vector3> s, AnimatableProperty<Quaternion> r, AnimatableProperty<Vector3> t)
        {
            this.name = name;
            this.thisIndex = thisIndex;
            this.parentIndex = parentIndex;
            this.childIndices = childIndices;

            this.localMatrix = localMatrix;
            this.useAnimatedTransforms = false;

            var ss = s != null && s.IsAnimated;
            var rr = r != null && r.IsAnimated;
            var tt = t != null && t.IsAnimated;

            if (!(ss || rr || tt))
            {
                useAnimatedTransforms = false;
                localScale = null;
                localRotation = null;
                localTranslation = null;
                return;
            }

            useAnimatedTransforms = true;
            localScale = s;
            localRotation = r;
            localTranslation = t;

            var m = Matrix4x4.Identity;
            if (s != null)
                m *= Matrix4x4.CreateScale(s.Value);

            if (r != null)
                m *= Matrix4x4.CreateFromQuaternion(r.Value);

            if (t != null)
                m.Translation = t.Value;

            this.localMatrix = m;
        }

        public AffineTransform GetLocalTransform()
        {
            var s = this.LocalScale?.Value;
            var r = this.LocalRotation?.Value;
            var t = this.LocalTranslation?.Value;

            return new AffineTransform(s, r, t);
        }

        public AffineTransform GetLocalTransform(int trackIndex, float time)
        {
            if (!this.useAnimatedTransforms || trackIndex < 0)
                return this.GetLocalTransform();

            var s = this.LocalScale?.GetValueAt(trackIndex, time);
            var r = this.LocalRotation?.GetValueAt(trackIndex, time);
            var t = this.LocalTranslation?.GetValueAt(trackIndex, time);

            return new AffineTransform(s, r, t);
        }

        public AffineTransform GetLocalTransform(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!this.useAnimatedTransforms)
                return this.GetLocalTransform();

            Span<AffineTransform> xforms = stackalloc AffineTransform[track.Length];

            for (int i = 0; i < xforms.Length; ++i)
                xforms[i] = this.GetLocalTransform(track[i], time[i]);

            return AffineTransform.Blend(xforms, weight);
        }

        public Matrix4x4 GetLocalMatrix(int trackIndex, float time)
        {
            if (!this.useAnimatedTransforms || trackIndex < 0)
                return this.LocalMatrix;

            return this.GetLocalTransform(trackIndex, time).Matrix;
        }

        public Matrix4x4 GetLocalMatrix(ReadOnlySpan<int> track, ReadOnlySpan<float> time, ReadOnlySpan<float> weight)
        {
            if (!this.useAnimatedTransforms)
                return this.LocalMatrix;

            return this.GetLocalTransform(track, time, weight).Matrix;
        }

        public override string ToString() => name;
    }
}
