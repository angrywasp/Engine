// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;
using System.Threading;

namespace Microsoft.Xna.Framework.Graphics
{
	public abstract class Texture : GraphicsResource
	{
		internal SurfaceFormat _format;
		internal int _levelCount;

        private readonly int _sortingKey = Interlocked.Increment(ref _lastSortingKey);
        private static int _lastSortingKey;

        internal int glTexture = -1;
        internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal PixelInternalFormat glInternalFormat;
        internal PixelFormat glFormat;
        internal PixelType glType;
        internal SamplerState glLastSamplerState;

        /// <summary>
        /// Gets a unique identifier of this texture for sorting purposes.
        /// </summary>
        /// <remarks>
        /// <para>For example, this value is used by <see cref="SpriteBatch"/> when drawing with <see cref="SpriteSortMode.Texture"/>.</para>
        /// <para>The value is an implementation detail and may change between application launches or MonoGame versions.
        /// It is only guaranteed to stay consistent during application lifetime.</para>
        /// </remarks>
        internal int SortingKey
        {
            get { return _sortingKey; }
        }

		public SurfaceFormat Format
		{
			get { return _format; }
		}
		
		public int LevelCount
		{
			get { return _levelCount; }
		}

        public static int CalculateMipLevels(int width, int height = 0, int depth = 0)
        {
            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);
            while (size > 1)
            {
                size = size / 2;
                levels++;
            }
            return levels;
        }

        public static void GetSizeForLevel(int width, int height, int level, out int w, out int h)
        {
            w = width;
            h = height;
            while (level > 0)
            {
                --level;
                w /= 2;
                h /= 2;
            }
            if (w == 0)
                w = 1;
            if (h == 0)
                h = 1;
        }

        public static void GetSizeForLevel(int width, int height, int depth, int level, out int w, out int h, out int d)
        {
            w = width;
            h = height;
            d = depth;
            while (level > 0)
            {
                --level;
                w /= 2;
                h /= 2;
                d /= 2;
            }
            if (w == 0)
                w = 1;
            if (h == 0)
                h = 1;
            if (d == 0)
                d = 1;
        }

        internal protected override void GraphicsDeviceResetting()
        {
            DeleteGLTexture();
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        private void DeleteGLTexture()
        {
            if (glTexture > 0)
            {
                GraphicsDevice.DisposeTexture(glTexture);
            }
            glTexture = -1;
        }
    }
}

