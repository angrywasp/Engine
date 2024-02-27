using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine.Content.Model;
using Engine.Content.Model.Template;
using Microsoft.Xna.Framework;
using SharpGLTF.Animations;

namespace Engine.AssetTransport
{
    public class SkeletonReader
    {
        public static (SkeletonTemplate Skeleton, SkinTemplate[] Skins) Read(BinaryReader br)
        {
            var trackCount = br.ReadInt32();
            var tracks = new AnimationTrackInfo[trackCount];

            for (int i = 0; i < trackCount; i++)
                tracks[i] = ReadAnimationTrack(br);

            var nodeCount = br.ReadInt32();
            var nodes = new SkeletonNodeTemplate[nodeCount];

            for (int i = 0; i < nodeCount; i++)
                nodes[i] = ReadSkeletonNode(br);

            var skin = ReadSkin(br);

            return (new SkeletonTemplate(nodes, tracks), skin);
        }

        private static AnimationTrackInfo ReadAnimationTrack(BinaryReader br)
        {
            var name = br.ReadString();
            var duration = br.ReadSingle();

            return new AnimationTrackInfo(name, duration);
        }

        private static SkeletonNodeTemplate ReadSkeletonNode(BinaryReader br)
        {
            var name = br.ReadString();
            var thisIndex = br.ReadInt32();
            var parentIndex = br.ReadInt32();
            var childIndicesCount = br.ReadInt32();
            var childIndices = MemoryMarshal.Cast<byte, int>(br.ReadBytes(childIndicesCount)).ToArray();
            var localMatrix = br.ReadMatrix();
            var scale = ReadAnimatableProperty(br, SamplerTraits.Vector3, () => br.ReadVector3(), (s) => s.CreateEvaluator());
            var rotation = ReadAnimatableProperty(br, SamplerTraits.Quaternion, () => br.ReadQuaternion(), (s) => s.CreateEvaluator());
            var translation = ReadAnimatableProperty(br, SamplerTraits.Vector3, () => br.ReadVector3(), (s) => s.CreateEvaluator());

            return new SkeletonNodeTemplate(name, thisIndex, parentIndex, childIndices, localMatrix, scale, rotation, translation);
        }

        private static AnimatableProperty<T> ReadAnimatableProperty<T>(BinaryReader br, ISamplerTraits<T> traits, Func<T> read, Func<FastCurveSampler<T>, ICurveEvaluator<T>> createEvaluator) where T : struct
        {
            var hasValue = br.ReadBoolean();
            if (!hasValue)
                return null;

            var property = new AnimatableProperty<T>(read());

            var curveCount = br.ReadInt32();
            if (curveCount == 0)
                return property;

            for (int a = 0; a < curveCount; a++)
            {
                var samplers = new ICurveSampler<T>[br.ReadInt32()];

                for (int b = 0; b < samplers.Length; b++)
                {
                    var sequence = new (float, T)[br.ReadInt32()];
                    for (int c = 0; c < sequence.Length; c++)
                        sequence[c] = (br.ReadSingle(), read());

                    samplers[b] = new LinearSampler<T>(sequence, traits);
                }

                var curveSampler = new FastCurveSampler<T>(samplers);
                property.SetCurve(a, createEvaluator(curveSampler));
            }

            return property;
        }

        private static SkinTemplate[] ReadSkin(BinaryReader br)
        {
            var skin = new SkinTemplate[br.ReadInt32()];

            for (int i = 0; i < skin.Length; i++)
            {
                var name = br.ReadString();

                int indicesLength = br.ReadInt32();
                var indices = MemoryMarshal.Cast<byte, int>(br.ReadBytes(indicesLength)).ToArray();

                int matricesLength = br.ReadInt32();
                var matrices = MemoryMarshal.Cast<byte, Matrix4x4>(br.ReadBytes(matricesLength)).ToArray();

                skin[i] = new SkinTemplate(name, indices, matrices);
            }

            return skin;
        }
    }
}
