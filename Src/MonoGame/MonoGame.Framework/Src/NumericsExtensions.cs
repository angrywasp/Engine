using System.Numerics;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework
{
    public static class Vector3Orientation
    {
        public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
        public static readonly Vector3 UnitX = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 UnitY = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 UnitZ = new Vector3(0f, 0f, 1f);
        public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 Down = new Vector3(0f, -1f, 0f);
        public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
        public static readonly Vector3 Forward = new Vector3(0f, 0f, -1f);
        public static readonly Vector3 Backward = new Vector3(0f, 0f, 1f);
    }

    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Up(this Vector3 v) => Vector3Orientation.Up;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Down(this Vector3 v) => Vector3Orientation.Down;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Left(this Vector3 v) => Vector3Orientation.Left;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Right(this Vector3 v) => Vector3Orientation.Right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Forward(this Vector3 v) => Vector3Orientation.Forward;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Backward(this Vector3 v) => Vector3Orientation.Backward;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this Vector3[] sourceArray, ref Matrix4x4 matrix, Vector3[] destinationArray)
        {
            for (var i = 0; i < sourceArray.Length; i++)
            {
                var position = sourceArray[i];
                destinationArray[i] = new Vector3(
                    (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                    (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                    (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43);
            }
        }
    }

    public static class Matrix4x4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Backward(this Matrix4x4 m) => new Vector3(m.M31, m.M32, m.M33);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Down(this Matrix4x4 m) => new Vector3(-m.M21, -m.M22, -m.M23);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Forward(this Matrix4x4 m) => new Vector3(-m.M31, -m.M32, -m.M33);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Left(this Matrix4x4 m) => new Vector3(-m.M11, -m.M12, -m.M13);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Right(this Matrix4x4 m) => new Vector3(m.M11, m.M12, m.M13);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Up(this Matrix4x4 m) => new Vector3(m.M21, m.M22, m.M23);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Backward(this Matrix4x4 m, Vector3 value)
        {
            m.M31 = value.X;
            m.M32 = value.Y;
            m.M33 = value.Z;
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Down(this Matrix4x4 m, Vector3 value)
        {
            m.M21 = -value.X;
            m.M22 = -value.Y;
            m.M23 = -value.Z;
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Forward(this Matrix4x4 m, Vector3 value)
        {
            m.M31 = -value.X;
            m.M32 = -value.Y;
            m.M33 = -value.Z;
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Left(this Matrix4x4 m, Vector3 value)
        {
            m.M11 = -value.X;
            m.M12 = -value.Y;
            m.M13 = -value.Z;
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Right(this Matrix4x4 m, Vector3 value)
        {
            m.M11 = value.X;
            m.M12 = value.Y;
            m.M13 = value.Z;
            return m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Up(this Matrix4x4 m, Vector3 value)
        {
            m.M21 = value.X;
            m.M22 = value.Y;
            m.M23 = value.Z;

            return m;
        }
    }
}