// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct Viewport
    {
		private int x;
		private int y;
		private int width;
		private int height;

        public int X => x;
        public int Y => y;
        public int Width => width;
        public int Height => height;
        public float MinDepth => 0.0f;
        public float MaxDepth => 1.0f;

        public Rectangle Bounds => new Rectangle(x, y, width, height);

		public float AspectRatio => (float) width / (float)height;

        public Viewport(int x, int y, int width, int height)
		{
			this.x = x;
		    this.y = y;
		    this.width = width;
		    this.height = height;
		}

        /// <summary>
        /// Projects a <see cref="Vector3"/> from world space into screen space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to project.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public Vector3 Project(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Matrix4x4 matrix = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector.X = vector.X / a;
		        vector.Y = vector.Y / a;
		        vector.Z = vector.Z / a;
		    }
		    vector.X = (((vector.X + 1f) * 0.5f) * this.width) + this.x;
		    vector.Y = (((-vector.Y + 1f) * 0.5f) * this.height) + this.y;
		    vector.Z = (vector.Z * (this.MaxDepth - this.MinDepth)) + this.MinDepth;
		    return vector;
        }

        /// <summary>
        /// Unprojects a <see cref="Vector3"/> from screen space into world space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to unproject.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Matrix4x4 matrix;
            Matrix4x4.Invert(Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection), out matrix);
		    source.X = (((source.X - this.x) / ((float) this.width)) * 2f) - 1f;
		    source.Y = -((((source.Y - this.y) / ((float) this.height)) * 2f) - 1f);
		    source.Z = (source.Z - this.MinDepth) / (this.MaxDepth - this.MinDepth);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector.X = vector.X / a;
		        vector.Y = vector.Y / a;
		        vector.Z = vector.Z / a;
		    }
		    return vector;

        }
		
		private static bool WithinEpsilon(float a, float b)
		{
		    float num = a - b;
		    return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}

        public override string ToString ()
	    {
	        return "{X:" + x + " Y:" + y + " Width:" + width + " Height:" + height + " MinDepth:" + MinDepth + " MaxDepth:" + MaxDepth + "}";
	    }
    }
}

