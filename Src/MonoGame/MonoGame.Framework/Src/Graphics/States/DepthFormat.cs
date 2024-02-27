// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines formats for depth-stencil buffer.
    /// </summary>
    public enum DepthFormat
    {
        None,
        Depth16 = RenderbufferStorage.DepthComponent16,
        Depth24 = RenderbufferStorage.DepthComponent24,
        Depth32 = RenderbufferStorage.DepthComponent32,
        Depth32F = RenderbufferStorage.DepthComponent32F,
        Depth24Stencil8 = RenderbufferStorage.Depth24Stencil8,
        Depth32FStencil8 = RenderbufferStorage.Depth32FStencil8
    }
}