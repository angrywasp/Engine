using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Content.Model;
using Engine.Content.Model.Template;
using SharpGLTF.Schema2;

namespace MeshProcessor.Skeleton
{
    public class SkeletonFactory
    {
        private List<SkeletonNodeTemplate> nodes = new List<SkeletonNodeTemplate>();
        private Dictionary<SharpGLTF.Schema2.Node, SkeletonNodeTemplate> map = new Dictionary<SharpGLTF.Schema2.Node, SkeletonNodeTemplate>();
        private SkeletonTemplate skeleton;
        private SkinTemplate[] skins;

        public Dictionary<SharpGLTF.Schema2.Node, SkeletonNodeTemplate> Map => map;
        public SkeletonTemplate Skeleton => skeleton;
        public SkinTemplate[] Skins => skins;

        public SkeletonFactory(ModelRoot srcModel)
        {
            AddSceneRoot(srcModel.LogicalScenes[0]);

            if (srcModel.LogicalAnimations.Count == 0)
                return;

            var animationTracks = new AnimationTrackInfo[srcModel.LogicalAnimations.Count];

            for (int i = 0; i < animationTracks.Length; ++i)
            {
                var track = srcModel.LogicalAnimations[i];
                animationTracks[i] = new AnimationTrackInfo(track.Name, track.Duration);
            }

            skeleton = new SkeletonTemplate(nodes.ToArray(), animationTracks);

            skins = map.Keys
                .Where(item => item.Mesh != null)
                .Select(item => CreateSkinTemplate(item)).ToArray();
        }

        private void AddRoot(SharpGLTF.Schema2.Node node) => AddNodeRescursive(node, -1);

        private SkeletonNodeTemplate GetNode(SharpGLTF.Schema2.Node srcNode) => map[srcNode];

        private SkinTemplate CreateSkinTemplate(SharpGLTF.Schema2.Node node)
        {
            var bones = new (SharpGLTF.Schema2.Node, Matrix4x4)[node.Skin.JointsCount];
            for (int i = 0; i < bones.Length; ++i)
                bones[i] = node.Skin.GetJoint(i);

            return new SkinTemplate(GetNode(node).Name, bones
                .Select(item => (GetNode(item.Item1), item.Item2))
                .ToArray());
        }

        private AnimatableProperty<Vector3> GetScale(SharpGLTF.Schema2.Node node)
        {
            var s = new AnimatableProperty<Vector3>(node.LocalTransform.Scale);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var evaluator = anim
                    .FindScaleChannel(node)
                    ?.GetScaleSampler()
                    ?.CreateCurveSampler(true)
                    .CreateEvaluator();

                if (evaluator != null)
                    s.SetCurve(anim.LogicalIndex, evaluator);
            }

            return s;
        }

        private AnimatableProperty<Quaternion> GetRotation(SharpGLTF.Schema2.Node node)
        {
            var r = new AnimatableProperty<Quaternion>(node.LocalTransform.Rotation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var a = anim.FindRotationChannel(node);
                var b = a?.GetRotationSampler();
                var c = b?.CreateCurveSampler(true);

                var evaluator = anim
                    .FindRotationChannel(node)
                    ?.GetRotationSampler()
                    ?.CreateCurveSampler(true)
                    .CreateEvaluator();

                if (evaluator != null)
                    r.SetCurve(anim.LogicalIndex, evaluator);
            }

            return r;
        }

        private AnimatableProperty<Vector3> GetTranslation(SharpGLTF.Schema2.Node node)
        {
            var t = new AnimatableProperty<Vector3>(node.LocalTransform.Translation);

            foreach (var anim in node.LogicalParent.LogicalAnimations)
            {
                var evaluator = anim
                    .FindTranslationChannel(node)
                    ?.GetTranslationSampler()
                    ?.CreateCurveSampler(true)
                    .CreateEvaluator();

                if (evaluator != null)
                    t.SetCurve(anim.LogicalIndex, evaluator);
            }

            return t;
        }

        private void AddSceneRoot(SharpGLTF.Schema2.Scene scene)
        {
            foreach (var root in scene.VisualChildren)
                AddRoot(root);
        }

        private int AddNodeRescursive(SharpGLTF.Schema2.Node src, int parentIndex)
        {
            var thisIdx = nodes.Count;
            nodes.Add(null);

            var childIndices = new List<int>();

            foreach (var child in src.VisualChildren)
                childIndices.Add(AddNodeRescursive(child, thisIdx));

            var dst = new SkeletonNodeTemplate(src.Name, thisIdx, parentIndex, childIndices.ToArray(),
                src.LocalMatrix, GetScale(src), GetRotation(src), GetTranslation(src));

            nodes[thisIdx] = dst;
            map[src] = dst;

            return thisIdx;
        }
    }
}
