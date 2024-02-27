namespace Microsoft.Xna.Framework
{
    public interface ICurveEvaluator<T>
    {
        T Evaluate(float position);
    }
}