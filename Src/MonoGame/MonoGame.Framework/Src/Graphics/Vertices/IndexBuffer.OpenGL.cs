﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
		internal int ibo;	

        private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
        {
            Threading.BlockOnUIThread(GenerateIfRequired);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            ibo = 0;
        }

        /// <summary>
        /// If the IBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (ibo == 0)
            {
                var sizeInBytes = IndexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

                GL.GenBuffers(1, out ibo);
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                              (IntPtr)sizeInBytes, IntPtr.Zero, _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.EnsureUIThread();
            GetBufferData(offsetInBytes, data, startIndex, elementCount);
        }

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GraphicsExtensions.CheckGLError();
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            IntPtr ptr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly);
            // Pointer to the start of data to read in the index buffer
            ptr = new IntPtr(ptr.ToInt64() + offsetInBytes);
			if (typeof(T) == typeof(byte))
            {
                byte[] buffer = data as byte[];
                // If data is already a byte[] we can skip the temporary buffer
                // Copy from the index buffer to the destination array
                Marshal.Copy(ptr, buffer, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            else
            {
                // Temporary buffer to store the copied section of data
                byte[] buffer = new byte[elementCount * elementSizeInByte];
                // Copy from the index buffer to the temporary buffer
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                // Copy from the temporary buffer to the destination array
                Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            Threading.EnsureUIThread();
            BufferData(offsetInBytes, data, startIndex, elementCount, options);
        }

        private void BufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            GenerateIfRequired();
            
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * elementCount;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
            var bufferSize = IndexCount * (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GraphicsExtensions.CheckGLError();
            
            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(  BufferTarget.ElementArrayBuffer,
                              (IntPtr)bufferSize,
                              IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
            
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
            GraphicsExtensions.CheckGLError();
            
            dataHandle.Free();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeBuffer(ibo);
            }
            base.Dispose(disposing);
        }
	}
}
