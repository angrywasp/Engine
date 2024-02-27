using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using GltfVector3CurveSampler = SharpGLTF.Animations.ICurveSampler<System.Numerics.Vector3>;
using GltfQuaternionCurveSampler = SharpGLTF.Animations.ICurveSampler<System.Numerics.Quaternion>;

namespace Engine.Content.Model
{
    public class AnimatableProperty<T> where T : struct
    {
        private List<ICurveEvaluator<T>> curves;
        private readonly T value;

        public IReadOnlyList<ICurveEvaluator<T>> Curves => curves;
        public T Value => value;

        public bool IsAnimated => curves == null ? false : curves.Count > 0;

        public AnimatableProperty(T defaultValue)
        {
            this.value = defaultValue;
        }

        public T GetValueAt(int curveIndex, float offset)
        {
            if (curves == null)
                return this.value;

            if (curveIndex < 0 || curveIndex >= curves.Count)
                return this.value;

            return curves[curveIndex].Evaluate(offset);
        }

        public void SetCurve(int curveIndex, ICurveEvaluator<T> evaluator)
        {
            if (curveIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(curveIndex));

            if (curves == null)
                curves = new List<ICurveEvaluator<T>>();

            while (curves.Count <= curveIndex)
                curves.Add(null);

            curves[curveIndex] = evaluator;
        }
    }

    public static class CurveEvaluatorExtensions
    {
        public static ICurveEvaluator<Vector3> CreateEvaluator(this GltfVector3CurveSampler curve)
        {
            if (curve == null)
                return null;

            return new Vector3CurveEvaluator(curve);
        }

        public static ICurveEvaluator<Quaternion> CreateEvaluator(this GltfQuaternionCurveSampler curve)
        {
            if (curve == null)
                return null;

            return new QuaternionCurveEvaluator(curve);
        }
    }

    public interface IGltfAnimationCurveSampler<T> : ICurveEvaluator<T>
    {
        SharpGLTF.Animations.ICurveSampler<T> Source { get; }
    }

    public struct Vector3CurveEvaluator : IGltfAnimationCurveSampler<Vector3>
    {
        private readonly GltfVector3CurveSampler source;

        public GltfVector3CurveSampler Source => source;

        public Vector3CurveEvaluator(GltfVector3CurveSampler source)
        {
            this.source = source;
        }

        public Vector3 Evaluate(float offset) => source.GetPoint(offset);
    }

    public struct QuaternionCurveEvaluator : IGltfAnimationCurveSampler<Quaternion>
    {
        private readonly GltfQuaternionCurveSampler source;

        public GltfQuaternionCurveSampler Source => source;

        public QuaternionCurveEvaluator(GltfQuaternionCurveSampler source)
        {
            this.source = source;
        }

        public Quaternion Evaluate(float offset) => source.GetPoint(offset);
    }
}
