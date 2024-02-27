using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Content.Model;
using Engine.Content.Model.Template;
using SharpGLTF.Animations;

namespace Engine.AssetTransport
{
    public class SkeletonWriter
    {
        public static void Write(BinaryWriter bw, SkeletonTemplate skeleton, SkinTemplate[] skin)
        {
            bw.Write(skeleton.AnimationTracks.Length);
            foreach (var track in skeleton.AnimationTracks)
                WriteAnimationTrack(bw, track);

            bw.Write(skeleton.NodesByName.Count);
            foreach (var node in skeleton.NodesByName.Values)
                WriteSkeletonNode(bw, node);

            WriteSkin(bw, skin);
        }

        private static void WriteAnimationTrack(BinaryWriter bw, AnimationTrackInfo track)
        {
            bw.Write(track.Name);
            bw.Write(track.Duration);
        }

        private static void WriteSkeletonNode(BinaryWriter bw, SkeletonNodeTemplate node)
        {
            var cb = MemoryMarshal.AsBytes<int>(node.ChildIndices);

            bw.Write(node.Name);
            bw.Write(node.ThisIndex);
            bw.Write(node.ParentIndex);
            bw.Write(cb.Length);
            bw.Write(cb);
            bw.Write(node.LocalMatrix);

            WriteAnimatableProperty(bw, node.LocalScale, v => bw.Write(v));
            WriteAnimatableProperty(bw, node.LocalRotation, v => bw.Write(v));
            WriteAnimatableProperty(bw, node.LocalTranslation, v => bw.Write(v));
        }

        private static void WriteAnimatableProperty<T>(BinaryWriter bw, AnimatableProperty<T> property, Action<T> write) where T : struct
        {
            if (property == null)
            {
                bw.Write(false);
                return;
            }

            bw.Write(true);
            write(property.Value);

            bw.Write(property.Curves == null || property.Curves.Count == 0 ? 0 : property.Curves.Count);

            if (property.Curves == null || property.Curves.Count == 0)
                return;

            foreach (var c in property.Curves)
            {
                if (c is not IGltfAnimationCurveSampler<T> ce)
                    throw new InvalidDataException($"Curve evaluator type {c.GetType()} is not supported");

                if (ce.Source is not FastCurveSampler<T> curveSampler)
                    throw new InvalidDataException($"Curve evaluator source type {ce.Source.GetType()} is not supported");

                bw.Write(curveSampler.Samplers.Length);

                foreach (var sampler in curveSampler.Samplers)
                {
                    if (sampler is not LinearSampler<T> linearSampler)
                        throw new InvalidDataException($"Curve sampler type {sampler.GetType()} is not supported");

                    bw.Write(linearSampler.Sequence.Count());

                    foreach (var s in linearSampler.Sequence)
                    {
                        bw.Write(s.Key);
                        write(s.Value);
                    }
                }
            }
        }

        private static void WriteSkin(BinaryWriter bw, SkinTemplate[] skin)
        {
            bw.Write(skin.Length);

            foreach (var s in skin)
            {
                bw.Write(s.Name);

                var indices = MemoryMarshal.AsBytes<int>(s.JointsNodeIndices);
                bw.Write(indices.Length);
                bw.Write(indices);

                var matrices = MemoryMarshal.AsBytes<Matrix4x4>(s.JointsBindMatrices);
                bw.Write(matrices.Length);
                bw.Write(matrices);
            }
        }
    }
}
