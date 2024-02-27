using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Content.Model.Template;

namespace Engine.Content.Model.Instance
{
    public class SkeletonInstance
    {
        private SkeletonTemplate template;
        private SkeletonNodeInstance[] nodes;

        public AnimationTrackInfo[] AnimationTracks => template.AnimationTracks;

        public SkeletonInstance(SkeletonTemplate template)
        {
            this.template = template;
            this.nodes = new SkeletonNodeInstance[template.Count];

            for (var i = 0; i < nodes.Length; ++i)
            {
                var n = template[i];
                var pidx = n.ParentIndex;
                var p = pidx < 0 ? null : nodes[pidx];
                nodes[i] = new SkeletonNodeInstance(n, p);
            }

            SetPoseTransforms();
        }

        public int IndexOfNode(string nodeName)
        {
            for (int i = 0; i < nodes.Length; ++i)
            {
                if (nodes[i].Name == nodeName)
                    return i;
            }

            return -1;
        }

        public void SetLocalMatrix(string name, Matrix4x4 localMatrix)
        {
            var n = nodes.FirstOrDefault(item => item.Name == name);

            if (n == null)
                return;

            n.LocalMatrix = localMatrix;
        }

        public Matrix4x4 GetModelMatrix(int index) => nodes[index].ModelMatrix;

        public void SetModelMatrix(string name, Matrix4x4 modelMatrix)
        {
            var n = nodes.FirstOrDefault(item => item.Name == name);
            
            if (n == null)
                return;

            n.ModelMatrix = modelMatrix;
        }

        public void SetPoseTransforms()
        {
            foreach (var n in nodes)
                n.SetPoseTransform();
        }

        public float GetAnimationDuration(int trackIndex) => template.GetTrackDuration(trackIndex);

        public int IndexOfTrack(string name) => template.IndexOfTrack(name);

        public void SetAnimationFrame(int trackIndex, float time, bool looped = true)
        {
            if (looped)
            {
                var duration = GetAnimationDuration(trackIndex);
                if (duration > 0)
                    time %= duration;
            }

            foreach (var n in nodes)
                n.SetAnimationFrame(trackIndex, time);
        }

        public void SetAnimationFrame(params (int TrackIndex, float Time, float Weight)[] blended)
        {
            Span<int> tracks = stackalloc int[blended.Length];
            Span<float> times = stackalloc float[blended.Length];
            Span<float> weights = stackalloc float[blended.Length];

            float w = blended.Sum(item => item.Weight);

            w = w == 0 ? 1 : 1 / w;

            for (int i = 0; i < blended.Length; ++i)
            {
                tracks[i] = blended[i].TrackIndex;
                times[i] = blended[i].Time;
                weights[i] = blended[i].Weight * w;
            }

            foreach (var n in nodes)
                n.SetAnimationFrame(tracks, times, weights);
        }
    }
}
