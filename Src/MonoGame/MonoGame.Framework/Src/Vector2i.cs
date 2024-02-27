using System.Runtime.InteropServices;
using System.Numerics;

namespace Microsoft.Xna.Framework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2i
    {
        public int X;

        public int Y;

        public static readonly Vector2i Zero = new Vector2i(0);
        public static readonly Vector2i One = new Vector2i(1);

        public Vector2i(int f)
        {
            X = f;
            Y = f;
        }

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector2i v1, Vector2i v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vector2i v1, Vector2i v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }

        public static Vector2i operator *(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector2i operator *(Vector2i v1, int i)
        {
            return new Vector2i(v1.X * i, v1.Y * i);
        }

        public static Vector2i operator /(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Vector2i operator /(Vector2i v1, int i)
        {
            return new Vector2i(v1.X / i, v1.Y / i);
        }

        public static Vector2i operator +(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2i operator +(Vector2i v1, int i)
        {
            return new Vector2i(v1.X + i, v1.Y + i);
        }

        public static Vector2i operator -(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2i operator -(Vector2i v1, int i)
        {
            return new Vector2i(v1.X - i, v1.Y - i);
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return ((obj is Vector2i) && (this == ((Vector2i)obj)));
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public static Vector2i FromVector2(Vector2 v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1}}}", X, Y);
        }
    }
}

