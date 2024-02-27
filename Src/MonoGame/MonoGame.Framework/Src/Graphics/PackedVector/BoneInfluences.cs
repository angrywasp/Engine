using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct BoneInfluences
    {
        public static BoneInfluences FromCollection(IEnumerable<(int Index, float Weight)> jointWeights)
        {
            if (jointWeights == null || !jointWeights.Any()) return default;

            int index = 0;

            var indices = Vector4.Zero;
            var weights = Vector4.Zero;
            var wsum = 0f;

            foreach (var (i, w) in jointWeights.OrderByDescending(item => item.Weight).Take(4))
            {
                switch (index)
                {
                    case 0: { indices.X = i; weights.X = w; wsum += w; break; }
                    case 1: { indices.Y = i; weights.Y = w; wsum += w; break; }
                    case 2: { indices.Z = i; weights.Z = w; wsum += w; break; }
                    case 3: { indices.W = i; weights.W = w; wsum += w; break; }
                }

                ++index;
            }

            return new BoneInfluences(indices, weights / wsum);
        }

        public static BoneInfluences Default = new BoneInfluences(Vector4.Zero, Vector4.UnitX);

        public BoneInfluences(Vector4 indices, Vector4 weights)
        {
            Indices = new Short4(indices);
            Weights = weights;
        }

        public BoneInfluences(Short4 indices, Vector4 weights)
        {
            Indices = indices;
            Weights = weights;
        }

        public Short4 Indices;
        public Vector4 Weights;

        public float WeightSum => Weights.X + Weights.Y + Weights.Z + Weights.W;

        public IEnumerable<(int,float)> GetIndexedWeights()
        {
            var indices = Indices.ToVector4();
            yield return ((int)indices.X, Weights.X);
            yield return ((int)indices.Y, Weights.Y);
            yield return ((int)indices.Z, Weights.Z);
            yield return ((int)indices.W, Weights.W);
        }
    }
}
