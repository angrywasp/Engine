// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.OpenGL
{
    public class GraphicsContext : IDisposable
    {
        private IntPtr glContext;
        private IntPtr winHandle;
        private bool disposed;

        public int SwapInterval
        {
            get => Sdl.GL.GetSwapInterval();
            set => Sdl.GL.SetSwapInterval(value);
        }

        public bool IsDisposed => disposed;

        public GraphicsContext(IntPtr windowHandle)
        {
            if (disposed)
                return;

            winHandle = windowHandle;
            glContext = Sdl.GL.CreateContext(windowHandle);
            OpenGL.GL.LoadEntryPoints();
        }

        public void MakeCurrent(IntPtr windowHandle)
        {
            if (disposed)
                return;
            
            winHandle = windowHandle;
            Sdl.GL.MakeCurrent(windowHandle, glContext);
        }

        public void SwapBuffers()
        {
            if (disposed)
                return;
            
            Sdl.GL.SwapWindow(winHandle);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            GraphicsDevice.DisposeContext(glContext);
            glContext = IntPtr.Zero;
            disposed = true;
        }
    }
}
