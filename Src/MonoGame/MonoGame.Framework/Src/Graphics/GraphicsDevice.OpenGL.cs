// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MonoGame.OpenGL;
using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        internal GraphicsContext Context { get; private set; }
        private DrawBuffersEnum[] _drawBuffers;

        private IntPtr windowHandle;

        enum ResourceType
        {
            Texture,
            Buffer,
            Shader,
            Program,
            Query,
            Framebuffer
        }

        struct ResourceHandle
        {
            public ResourceType type;
            public int handle;

            public static ResourceHandle Texture(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Texture, handle = handle };
            }

            public static ResourceHandle Buffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Buffer, handle = handle };
            }

            public static ResourceHandle Shader(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Shader, handle = handle };
            }

            public static ResourceHandle Program(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Program, handle = handle };
            }

            public static ResourceHandle Query(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Query, handle = handle };
            }

            public static ResourceHandle Framebuffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Framebuffer, handle = handle };
            }

            public void Free()
            {
                switch (type)
                {
                    case ResourceType.Texture:
                        GL.DeleteTextures(1, ref handle);
                        break;
                    case ResourceType.Buffer:
                        GL.DeleteBuffers(1, ref handle);
                        break;
                    case ResourceType.Shader:
                        if (GL.IsShader(handle))
                            GL.DeleteShader(handle);
                        break;
                    case ResourceType.Program:
                        if (GL.IsProgram(handle))
                            GL.DeleteProgram(handle);
                        break;
                    case ResourceType.Query:
                        GL.DeleteQueries(1, ref handle);
                        break;
                    case ResourceType.Framebuffer:
                        GL.DeleteFramebuffers(1, ref handle);
                        break;
                }
                GraphicsExtensions.CheckGLError();
            }
        }

        List<ResourceHandle> _disposeThisFrame = new List<ResourceHandle>();
        List<ResourceHandle> _disposeNextFrame = new List<ResourceHandle>();
        object _disposeActionsLock = new object();

        static List<IntPtr> _disposeContexts = new List<IntPtr>();
        static object _disposeContextsLock = new object();

        private static BufferBindingInfo[] _bufferBindingInfos;
        private static int _activeBufferBindingInfosCount;
        private static bool[] _newEnabledVertexAttributes;
        internal static readonly List<int> _enabledVertexAttributes = new List<int>();
        internal static bool _attribsDirty;

        internal FramebufferHelper framebufferHelper;

        internal int glMajorVersion = 0;
        internal int glMinorVersion = 0;
        internal int glFramebuffer = 0;
        internal int MaxVertexAttributes;
        internal int _maxTextureSize = 0;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal bool _lastBlendEnable = false;
        internal BlendState _lastBlendState = new BlendState();
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();
        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1.0f;
        private int _lastClearStencil = 0;

        private int maxTessellationPatchVertices;
        private int maxTessellationGenLevel;

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int ShaderProgramHash
        {
            get
            {
                if (_vertexShader == null)
                    throw new InvalidOperationException("No vertex shader bound!");
                if (_pixelShader == null)
                    throw new InvalidOperationException("No pixel shader bound!");

                return _vertexShader.HashKey ^ _pixelShader.HashKey;
            }
        }

        public void CheckTessellationSupport()
        {
            GL.GetInteger((int)GLPatchParameter.MaxPatchVertices, out maxTessellationPatchVertices);
            GL.GetInteger((int)GLPatchParameter.MaxTessGenLevel, out maxTessellationGenLevel);
        }

        public void SetPatchVertexCount(int count)
        {
            GL.PatchParameteri(GLPatchParameter.PatchVertices, count);
            GL.GetError();

            GL.GetInteger((int)GLPatchParameter.PatchVertices, out int patchVertices);
            
            if (patchVertices != count)
                throw new InvalidOperationException("Failed to set patch vertex count");
        }

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var x = 0; x < attrs.Length; x++)
            {
                if (attrs[x] && !_enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Add(x);
                    GL.EnableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
                else if (!attrs[x] && _enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Remove(x);
                    GL.DisableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void ApplyAttribs(VertexShader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var bindingsChanged = false;

            int attributeStartIndex = 0;
            for (var slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                var vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, attributeStartIndex, programHash);

                var vertexStride = vertexDeclaration.VertexStride;
                var offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    slot < _activeBufferBindingInfosCount &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, attrInfo) &&
                    _bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    _bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.vbo)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer.vbo);
                GraphicsExtensions.CheckGLError();

                // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                if (!GraphicsCapabilities.SupportsInstancing && vertexBufferBinding.InstanceFrequency > 0)
                    throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");

                foreach (var element in attrInfo.Elements)
                {
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        (IntPtr)(offset.ToInt64() + element.Offset));

                    // only set the divisor if instancing is supported
                    if (GraphicsCapabilities.SupportsInstancing) 
                        GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);

                    GraphicsExtensions.CheckGLError();
                }

                _bufferBindingInfos[slot].VertexOffset = offset;
                _bufferBindingInfos[slot].AttributeInfo = attrInfo;
                _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.vbo;

                attributeStartIndex += vertexDeclaration.InternalVertexElements.Length;
            }

            _attribsDirty = false;

            if (bindingsChanged)
            {
                Array.Clear(_newEnabledVertexAttributes, 0, _newEnabledVertexAttributes.Length);
                for (var slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    foreach (var element in _bufferBindingInfos[slot].AttributeInfo.Elements)
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                }
                _activeBufferBindingInfosCount = _vertexBuffers.Count;
            }
            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        private void PlatformSetup(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;

            if (Context == null || Context.IsDisposed)
                Context = GL.CreateContext(windowHandle);

            Context.MakeCurrent(windowHandle);
            Context.SwapInterval = PresentationParameters.PresentationInterval.GetSwapInterval();

            Context.MakeCurrent(windowHandle);

            MaxTextureSlots = 16;
            MaxVertexTextureSlots = 16;

            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];

            try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
                glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
            }
            catch (FormatException)
            {
                // if it fails, we assume to be on a 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }

			// Initialize draw buffer attachment array
			int maxDrawBuffers;
            GL.GetInteger(GetPName.MaxDrawBuffers, out maxDrawBuffers);
            GraphicsExtensions.CheckGLError ();
			_drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
			for (int i = 0; i < maxDrawBuffers; i++)
				_drawBuffers[i] = (DrawBuffersEnum)(FramebufferAttachment.ColorAttachment0Ext + i);
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            //_enabledVertexAttributes.Clear();
            framebufferHelper = FramebufferHelper.Create(this);

            // Force resetting states
            this.PlatformApplyBlend(true);
            this.DepthStencilState.PlatformApplyState(this, true);
            this.RasterizerState.PlatformApplyState(this, true);

            _bufferBindingInfos = new BufferBindingInfo[_maxVertexBufferSlots];
            for (int i = 0; i < _bufferBindingInfos.Length; i++)
                _bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, -1);

            CheckTessellationSupport();
        }
        
        private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = this.clearDepthStencilState;
		    BlendState = BlendState.Opaque;
            ApplyState(false);

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if (color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            }
			if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
				    GL.ClearStencil(stencil);
                    GraphicsExtensions.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
			}

			if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer) 
            {
                if (depth != _lastClearDepth)
                {
                    GL.ClearDepth(depth);
                    GraphicsExtensions.CheckGLError();
                    _lastClearDepth = depth;
                }
				bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
			}

            GL.Clear(bufferMask);
            GraphicsExtensions.CheckGLError();
           		
            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
            Context.Dispose();
            Context = null;
        }

        internal void DisposeTexture(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Texture(handle));
                }
            }
        }

        internal void DisposeBuffer(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Buffer(handle));
                }
            }
        }

        public void DisposeShader(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Shader(handle));
                }
            }
        }

        public void DisposeProgram(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Program(handle));
                }
            }
        }

        internal void DisposeQuery(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Query(handle));
                }
            }
        }

        internal void DisposeFramebuffer(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Framebuffer(handle));
                }
            }
        }

        static internal void DisposeContext(IntPtr resource)
        {
            lock (_disposeContextsLock)
            {
                _disposeContexts.Add(resource);
            }
        }

        static internal void DisposeContexts()
        {
            lock (_disposeContextsLock)
            {
                int count = _disposeContexts.Count;
                for (int i = 0; i < count; ++i)
                    Sdl.GL.DeleteContext(_disposeContexts[i]);
                _disposeContexts.Clear();
            }
        }

        public void PlatformPresent()
        {
            Context.SwapBuffers();
            GraphicsExtensions.CheckGLError();

            // Dispose of any GL resources that were disposed in another thread
            int count = _disposeThisFrame.Count;
            for (int i = 0; i < count; ++i)
                _disposeThisFrame[i].Free();
            _disposeThisFrame.Clear();

            lock (_disposeActionsLock)
            {
                // Swap lists so resources added during this draw will be released after the next draw
                var temp = _disposeThisFrame;
                _disposeThisFrame = _disposeNextFrame;
                _disposeNextFrame = temp;
            }
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            if (IsRenderTargetBound)
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            else
                GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(value.MinDepth, value.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            //Textures.Dirty();
        }

        private class RenderTargetBindingArrayComparer : IEqualityComparer<RenderTargetBinding[]>
        {
            public bool Equals(RenderTargetBinding[] first, RenderTargetBinding[] second)
            {
                if (object.ReferenceEquals(first, second))
                    return true;

                if (first == null || second == null)
                    return false;

                if (first.Length != second.Length)
                    return false;

                for (var i = 0; i < first.Length; ++i)
                {
                    if ((first[i].RenderTarget != second[i].RenderTarget) || (first[i].ArraySlice != second[i].ArraySlice)  || (first[i].MipLevel != second[i].MipLevel))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(RenderTargetBinding[] array)
            {
                if (array != null)
                {
                    unchecked
                    {
                        int hash = 17;
                        foreach (var item in array)
                        {
                            if (item.RenderTarget != null)
                                hash = hash * 23 + item.RenderTarget.GetHashCode();
                            hash = hash * 23 + item.ArraySlice.GetHashCode();
                            hash = hash * 23 + item.MipLevel.GetHashCode();
                        }
                        return hash;
                    }
                }
                return 0;
            }
        }

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        private Dictionary<RenderTargetBinding[], int> glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

        internal void PlatformCreateRenderTarget(IRenderTarget renderTarget, int width, int height, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            
            if (preferredMultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                this.framebufferHelper.GenRenderbuffer(out color);
                this.framebufferHelper.BindRenderbuffer(color);
                this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)RenderbufferStorage.Rgba8, width, height);
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = (RenderbufferStorage)preferredDepthFormat;
                var stencilInternalFormat = (RenderbufferStorage)0;

                if (depthInternalFormat != 0)
                {
                    this.framebufferHelper.GenRenderbuffer(out depth);
                    this.framebufferHelper.BindRenderbuffer(depth);
                    this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)depthInternalFormat, width, height);
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8 || preferredDepthFormat == DepthFormat.Depth32FStencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            this.framebufferHelper.GenRenderbuffer(out stencil);
                            this.framebufferHelper.BindRenderbuffer(stencil);
                            this.framebufferHelper.RenderbufferStorageMultisample(preferredMultiSampleCount, (int)stencilInternalFormat, width, height);
                        }
                    }
                }
            }

            if (color != 0)
                renderTarget.GLColorBuffer = color;
            else
                renderTarget.GLColorBuffer = renderTarget.GLTexture;
                
            renderTarget.GLDepthBuffer = depth;
            renderTarget.GLStencilBuffer = stencil;
        }

        internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
        {
            var color = 0;
            var depth = 0;
            var stencil = 0;
            var colorIsRenderbuffer = false;

            color = renderTarget.GLColorBuffer;
            depth = renderTarget.GLDepthBuffer;
            stencil = renderTarget.GLStencilBuffer;
            colorIsRenderbuffer = color != renderTarget.GLTexture;

            if (color != 0)
            {
                if (colorIsRenderbuffer)
                    this.framebufferHelper.DeleteRenderbuffer(color);
                if (stencil != 0 && stencil != depth)
                    this.framebufferHelper.DeleteRenderbuffer(stencil);
                if (depth != 0)
                    this.framebufferHelper.DeleteRenderbuffer(depth);

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in this.glFramebuffers.Keys)
                {
                    foreach (var binding in bindings)
                    {
                        if (binding.RenderTarget == renderTarget)
                        {
                            bindingsToDelete.Add(bindings);
                            break;
                        }
                    }
                }

                foreach (var bindings in bindingsToDelete)
                {
                    var fbo = 0;
                    if (this.glFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glFramebuffers.Remove(bindings);
                    }
                    if (this.glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        this.framebufferHelper.DeleteFramebuffer(fbo);
                        this.glResolveFramebuffers.Remove(bindings);
                    }
                }
            }
        }

        private void PlatformResolveRenderTargets()
        {
            if (this._currentRenderTargetCount == 0)
                return;

            var renderTargetBinding = this._currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0 && this.framebufferHelper.SupportsBlitFramebuffer)
            {
                var glResolveFramebuffer = 0;
                if (!this.glResolveFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glResolveFramebuffer))
                {
                    this.framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                    for (var i = 0; i < this._currentRenderTargetCount; ++i)
                    {
                        var rt = this._currentRenderTargetBindings[i].RenderTarget as IRenderTarget;
                        this.framebufferHelper.FramebufferTexture2D((int)(FramebufferAttachment.ColorAttachment0 + i), (int) rt.GetFramebufferTarget(renderTargetBinding), rt.GLTexture, renderTargetBinding.MipLevel);
                    }
                    this.glResolveFramebuffers.Add((RenderTargetBinding[])this._currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    this.framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                }
                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                if (this._lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
                var glFramebuffer = this.glFramebuffers[this._currentRenderTargetBindings];
                this.framebufferHelper.BindReadFramebuffer(glFramebuffer);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
                    this.framebufferHelper.BlitFramebuffer(i, renderTarget.Width, renderTarget.Height);
                }
                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.framebufferHelper.SupportsInvalidateFramebuffer)
                    this.framebufferHelper.InvalidateReadFramebuffer();
                if (this._lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
            }
            for (var i = 0; i < this._currentRenderTargetCount; ++i)
            {
                renderTargetBinding = this._currentRenderTargetBindings[i];
                renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
                if (renderTarget.LevelCount > 1)
                {
                    GL.BindTexture((TextureTarget)renderTarget.GLTarget, renderTarget.GLTexture);
                    GraphicsExtensions.CheckGLError();
                    this.framebufferHelper.GenerateMipmap((int)renderTarget.GLTarget);
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            var glFramebuffer = 0;
            if (!this.glFramebuffers.TryGetValue(this._currentRenderTargetBindings, out glFramebuffer))
            {
                this.framebufferHelper.GenFramebuffer(out glFramebuffer);
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
                var renderTargetBinding = this._currentRenderTargetBindings[0];
                var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.DepthAttachment, renderTarget.GLDepthBuffer, renderTargetBinding.MipLevel);
                this.framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.StencilAttachment, renderTarget.GLStencilBuffer, renderTargetBinding.MipLevel);
                for (var i = 0; i < this._currentRenderTargetCount; ++i)
                {
                    renderTargetBinding = this._currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
                    var attachement = (int)(FramebufferAttachment.ColorAttachment0 + i);
                    if (renderTarget.GLColorBuffer != renderTarget.GLTexture)
                        this.framebufferHelper.FramebufferRenderbuffer(attachement, renderTarget.GLColorBuffer, renderTargetBinding.MipLevel);
                    else
                        this.framebufferHelper.FramebufferTexture2D(attachement, (int)renderTarget.GetFramebufferTarget(renderTargetBinding), renderTarget.GLTexture, renderTargetBinding.MipLevel, renderTarget.MultiSampleCount);
                }

                this.glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                this.framebufferHelper.BindFramebuffer(glFramebuffer);
            }

            GL.DrawBuffers(this._currentRenderTargetCount, this._drawBuffers);

            _rasterizerStateDirty = true;
            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(this, force);
            ApplyBlendFactor(force);
        }

        private void ApplyBlendFactor(bool force)
        {
            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            if ( _scissorRectangleDirty )
	        {
                var scissorRect = _scissorRectangle;
                if (!IsRenderTargetBound)
                    scissorRect.Y = PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GraphicsExtensions.CheckGLError();
	            _scissorRectangleDirty = false;
	        }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                if (_indexBuffer != null)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer.ibo);
                    GraphicsExtensions.CheckGLError();
                }
                _indexBufferDirty = false;
            }
        }

        internal void OnPresentationChanged()
        {
            Context.MakeCurrent(windowHandle);
            Context.SwapInterval = PresentationParameters.PresentationInterval.GetSwapInterval();
            ApplyRenderTargets(null);
        }

        // Holds information for caching
        private class BufferBindingInfo
        {
            public VertexDeclaration.VertexDeclarationAttributeInfo AttributeInfo;
            public IntPtr VertexOffset;
            public int InstanceFrequency;
            public int Vbo;

            public BufferBindingInfo(VertexDeclaration.VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, int vbo)
            {
                AttributeInfo = attributeInfo;
                VertexOffset = vertexOffset;
                InstanceFrequency = instanceFrequency;
                Vbo = vbo;
            }
        }

        private void GetModeSwitchedSize(out int width, out int height)
        {
            var mode = new Sdl.Display.Mode
            {
                Width = PresentationParameters.BackBufferWidth,
                Height = PresentationParameters.BackBufferHeight,
                Format = 0,
                RefreshRate = 0,
                DriverData = IntPtr.Zero
            };
            Sdl.Display.Mode closest;
            Sdl.Display.GetClosestDisplayMode(0, mode, out closest);
            width = closest.Width;
            height = closest.Height;
        }

        private void GetDisplayResolution(out int width, out int height)
        {
            Sdl.Display.Mode mode;
            Sdl.Display.GetCurrentDisplayMode(0, out mode);
            width = mode.Width;
            height = mode.Height;
        }
    }
}
