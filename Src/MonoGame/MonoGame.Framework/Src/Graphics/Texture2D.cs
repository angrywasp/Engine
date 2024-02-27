// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        internal protected enum SurfaceType
        {
            Texture,
            RenderTarget,
            SwapChainRenderTarget,
        }

        internal int width;
        internal int height;

        internal float TexelWidth { get; private set; }
        internal float TexelHeight { get; private set; }

        /// <summary>
        /// Gets the dimensions of the texture
        /// </summary>
        public Rectangle Bounds => new Rectangle(0, 0, this.width, this.height);

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Rgba, SurfaceType.Texture)
        {
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture) { }

        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", "Texture width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", "Texture height must be greater than zero");

            this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.TexelWidth = 1f / (float)width;
            this.TexelHeight = 1f / (float)height;

            this._format = format;
            this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;

            // Texture will be assigned by the swap chain.
            if (type == SurfaceType.SwapChainRenderTarget)
                return;

            this.glTarget = TextureTarget.Texture2D;
            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

            Threading.EnsureUIThread();

            GenerateGLTextureIfRequired();
            int w = width;
            int h = height;
            int level = 0;
            while (true)
            {
                GL.TexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, glFormat, glType, IntPtr.Zero);
                GraphicsExtensions.CheckGLError();

                if ((w == 1 && h == 1) || !mipmap)
                    break;
                if (w > 1)
                    w = w / 2;
                if (h > 1)
                    h = h / 2;
                ++level;
            }
        }

        public int Width => width;

        public int Height => height;

        public void SetData<T>(T[] data) where T : struct => this.SetData<T>(0, null, data, 0, data.Length);

        public void SetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct => this.SetData<T>(level, null, data, startIndex, elementCount);

        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            ValidateParams(level, rect, data, startIndex, elementCount, out Rectangle checkedRect);
            PlatformSetData(level, checkedRect, data, startIndex, elementCount);
        }

        public void GetData<T>(T[] data) where T : struct => this.GetData(0, null, data, 0, data.Length);

        public void GetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct => this.GetData(level, null, data, startIndex, elementCount);

        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            ValidateParams(level, rect, data, startIndex, elementCount, out Rectangle checkedRect);
            PlatformGetData(level, checkedRect, data, startIndex, elementCount);
        }

        //Converts Pixel Data from ARGB to ABGR
        private static void ConvertToABGR(int pixelHeight, int pixelWidth, int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for (int i = 0; i < pixelCount; ++i)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }

        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture < 0)
            {
                GL.GenTextures(1, out this.glTexture);
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                var wrap = TextureWrapMode.Repeat;
                if (((width & (width - 1)) != 0) || ((height & (height - 1)) != 0))
                    wrap = TextureWrapMode.ClampToEdge;

                GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                                (_levelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                                (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)wrap);
                GraphicsExtensions.CheckGLError();
                // Set mipmap levels
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GraphicsExtensions.CheckGLError();
                if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
                {
                    if (_levelCount > 0)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, _levelCount - 1);
                    }
                    else
                    {
                        GL.TexParameter(TextureTarget.Texture2D, SamplerState.TextureParameterNameTextureMaxLevel, 1000);
                    }
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void ValidateParams<T>(int level, Rectangle? rect, T[] data,
            int startIndex, int elementCount, out Rectangle checkedRect) where T : struct
        {
            var textureBounds = new Rectangle(0, 0, Math.Max(width >> level, 1), Math.Max(height >> level, 1));
            checkedRect = rect ?? textureBounds;
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.", "level");
            if (!textureBounds.Contains(checkedRect) || checkedRect.Width <= 0 || checkedRect.Height <= 0)
                throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
            if (data == null)
                throw new ArgumentNullException("data");
            var tSize = ReflectionHelpers.SizeOf<T>();
            var fSize = Format.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");

            int dataByteSize = checkedRect.Width * checkedRect.Height * fSize;

            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1}.",
                                            elementCount * tSize, dataByteSize), "elementCount");
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            int w, h;
            GetSizeForLevel(Width, Height, level, out w, out h);

            Threading.EnsureUIThread();

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                // Store the current bound texture.
                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                if (prevTexture != glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GenerateGLTextureIfRequired();
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                GL.TexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, glFormat, glType, dataPtr);
                GraphicsExtensions.CheckGLError();

                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();

                // Restore the bound texture.
                if (prevTexture != glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformSetData<T>(int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.EnsureUIThread();

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                // Store the current bound texture.
                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                if (prevTexture != glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, glTexture);
                    GraphicsExtensions.CheckGLError();
                }

                GenerateGLTextureIfRequired();
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(_format.GetSize(), 8));

                GL.TexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height, glFormat, glType, dataPtr);

                GraphicsExtensions.CheckGLError();

                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
                GraphicsExtensions.CheckGLError();

                // Restore the bound texture.
                if (prevTexture != glTexture)
                {
                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.EnsureUIThread();

            var tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
            GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSizeInByte, 8));

            // we need to convert from our format size to the size of T here
            var tFullWidth = Math.Max(this.width >> level, 1) * Format.GetSize() / tSizeInByte;
            var temp = new T[Math.Max(this.height >> level, 1) * tFullWidth];
            GL.GetTexImage(TextureTarget.Texture2D, level, glFormat, glType, temp);
            GraphicsExtensions.CheckGLError();

            var pixelToT = Format.GetSize() / tSizeInByte;
            var rowCount = rect.Height;
            var tRectWidth = rect.Width * pixelToT;
            for (var r = 0; r < rowCount; r++)
            {
                var tempStart = rect.X * pixelToT + (r + rect.Top) * tFullWidth;
                var dataStart = startIndex + r * tRectWidth;
                Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
            }
        }
    }
}
