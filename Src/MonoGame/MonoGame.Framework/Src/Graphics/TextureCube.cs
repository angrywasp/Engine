// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public class TextureCube : Texture
    {
        internal int size;

        public int Size
        {
            get
            {
                return size;
            }
        }

        public TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, size, mipMap, format, false)
        {
        }

        internal TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", "Cube size must be greater than zero");

            this.GraphicsDevice = graphicsDevice;
            this.size = size;
            this._format = format;
            this._levelCount = mipMap ? CalculateMipLevels(size) : 1;

            Threading.EnsureUIThread();

            this.glTarget = TextureTarget.TextureCubeMap;

            GL.GenTextures(1, out this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                            mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
                            (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
                            (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
                            (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();

            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

            for (var i = 0; i < 6; i++)
            {
                var target = TextureTarget.TextureCubeMapPositiveX + i;

                GL.TexImage2D(target, 0, glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
                GraphicsExtensions.CheckGLError();
            }

            if (mipMap)
            {
                GraphicsDevice.FramebufferHelper.Get().GenerateMipmap((int)glTarget);
                // This updates the mipmaps after a change in the base texture
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)Bool.True);
                GraphicsExtensions.CheckGLError();
            }
        }

        public void GetData<T>(CubeMapFace cubeMapFace, T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            GetData(cubeMapFace, 0, null, data, 0, data.Length);
        }

        public void GetData<T>(CubeMapFace cubeMapFace, T[] data, int startIndex, int elementCount) where T : struct
        {
            GetData(cubeMapFace, 0, null, data, startIndex, elementCount);
        }

        public void GetData<T>(CubeMapFace cubeMapFace, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(level, rect, data, startIndex, elementCount, out checkedRect);

            Threading.EnsureUIThread();

            var target = (TextureTarget)cubeMapFace;
            var tSizeInByte = ReflectionHelpers.SizeOf<T>();
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);

            // we need to convert from our format size to the size of T here
            var tFullWidth = Math.Max(this.size >> level, 1) * Format.GetSize() / tSizeInByte;
            var temp = new T[Math.Max(this.size >> level, 1) * tFullWidth];
            GL.GetTexImage(target, level, glFormat, glType, temp);
            GraphicsExtensions.CheckGLError();

            var pixelToT = Format.GetSize() / tSizeInByte;
            var rowCount = checkedRect.Height;
            var tRectWidth = checkedRect.Width * pixelToT;
            for (var r = 0; r < rowCount; r++)
            {
                var tempStart = checkedRect.X * pixelToT + (r + checkedRect.Top) * tFullWidth;
                var dataStart = startIndex + r * tRectWidth;
                Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
            }
        }

        public void SetData<T>(CubeMapFace face, T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            SetData(face, 0, null, data, 0, data.Length);
        }

        public void SetData<T>(CubeMapFace face, T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData(face, 0, null, data, startIndex, elementCount);
        }

        public void SetData<T>(CubeMapFace face, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Rectangle checkedRect;
            ValidateParams(level, rect, data, startIndex, elementCount, out checkedRect);

            Threading.EnsureUIThread();

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var startBytes = startIndex * elementSizeInByte;

            try
            {
                var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
                GraphicsExtensions.CheckGLError();

                var target = (TextureTarget)face;
                GL.TexSubImage2D(target, level, checkedRect.X, checkedRect.Y, checkedRect.Width, checkedRect.Height, glFormat, glType, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void ValidateParams<T>(int level, Rectangle? rect, T[] data, int startIndex,
            int elementCount, out Rectangle checkedRect) where T : struct
        {
            var textureBounds = new Rectangle(0, 0, Math.Max(Size >> level, 1), Math.Max(Size >> level, 1));
            checkedRect = rect ?? textureBounds;
            if (level < 0 || level >= LevelCount)
                throw new ArgumentException("level must be smaller than the number of levels in this texture.");
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
    }
}

