// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;
using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice : IDisposable
    {
        public static readonly Vector4 DiscardDefault = Color.CornflowerBlue.ToVector4();
        public static readonly Vector4 DiscardBlack = Color.Black.ToVector4();
        public static readonly Vector4 DiscardWhite = Color.White.ToVector4();

        private PresentationParameters presentationParameters;
        private GraphicsCapabilities graphicsCapabilities;

        public PresentationParameters PresentationParameters => presentationParameters;
        public GraphicsCapabilities GraphicsCapabilities => graphicsCapabilities;

        private Viewport _viewport;

        private bool _isDisposed;

        private Color _blendFactor = Color.White;

        private BlendState _blendState;
        private BlendState _actualBlendState;
        private bool _blendStateDirty;

        private BlendState _blendStateAdditive;
        private BlendState _blendStateAlphaBlend;
        private BlendState _blendStateNonPremultiplied;
        private BlendState _blendStateOpaque;

        private DepthStencilState _depthStencilState;
        private DepthStencilState _actualDepthStencilState;
        private bool _depthStencilStateDirty;

        private DepthStencilState _depthStencilStateDefault;
        private DepthStencilState _depthStencilStateDepthRead;
        private DepthStencilState _depthStencilStateNone;

        private RasterizerState _rasterizerState;
        private RasterizerState _actualRasterizerState;
        private bool _rasterizerStateDirty;

        private RasterizerState _rasterizerStateCullClockwise;
        private RasterizerState _rasterizerStateCullCounterClockwise;
        private RasterizerState _rasterizerStateCullNone;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;

        private VertexBufferBindings _vertexBuffers;
        private bool _vertexBuffersDirty;

        private IndexBuffer _indexBuffer;
        private bool _indexBufferDirty;

        private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[8];
        private int _currentRenderTargetCount;
        private readonly RenderTargetBinding[] _tempRenderTargetBinding = new RenderTargetBinding[1];

        private readonly RenderTargetBinding[] _tempRenderTargetCubeBinding = new RenderTargetBinding[6];

        private VertexShader _vertexShader;
        private VertexShader _tessellationControlShader;
        private VertexShader _tessellationEvaluationShader;
        private PixelShader _pixelShader;

        internal Dictionary<int, Effect> EffectCache;

        private readonly object _resourcesLock = new object();

        // Use WeakReference for the global resources list as we do not know when a resource
        // may be disposed and collected. We do not want to prevent a resource from being
        // collected by holding a strong reference to it in this list.
        private readonly List<WeakReference> _resources = new List<WeakReference>();

        internal event EventHandler<PresentationParameters> PresentationChanged;

        private int _maxVertexBufferSlots;
        internal int MaxTextureSlots;
        internal int MaxVertexTextureSlots;

        public bool IsDisposed => _isDisposed;

        public bool IsContentLost => IsDisposed;

        internal bool IsRenderTargetBound => _currentRenderTargetCount > 0;

        internal DepthFormat ActiveDepthFormat
        {
            get
            {
                return IsRenderTargetBound
                    ? _currentRenderTargetBindings[0].DepthFormat
                    : PresentationParameters.DepthStencilFormat;
            }
        }

        public GraphicsAdapter Adapter
        {
            get;
            private set;
        }

        public GraphicsDevice(GraphicsDeviceInformation gdi)
            : this(gdi.Adapter, gdi.PresentationParameters)
        {
        }

        public GraphicsDevice()
        {
            this.presentationParameters = new PresentationParameters();
            this.presentationParameters.DepthStencilFormat = DepthFormat.Depth24;
            Setup();
            this.graphicsCapabilities = new GraphicsCapabilities();
            this.graphicsCapabilities.Initialize(this);
            Initialize();
        }

        public GraphicsDevice(GraphicsAdapter adapter, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");
            Adapter = adapter;
            this.presentationParameters = presentationParameters;
            Setup();
            this.graphicsCapabilities = new GraphicsCapabilities();
            this.graphicsCapabilities.Initialize(this);

            Initialize();
        }

        private void Setup()
        {
            // Initialize the main viewport
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            PlatformSetup(PresentationParameters.WindowHandle);

            _blendStateAdditive = BlendState.Additive.Clone();
            _blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
            _blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
            _blendStateOpaque = BlendState.Opaque.Clone();

            BlendState = BlendState.Opaque;

            _depthStencilStateDefault = DepthStencilState.Default.Clone();
            _depthStencilStateDepthRead = DepthStencilState.DepthRead.Clone();
            _depthStencilStateNone = DepthStencilState.None.Clone();

            DepthStencilState = DepthStencilState.Default;

            _rasterizerStateCullClockwise = RasterizerState.CullClockwise.Clone();
            _rasterizerStateCullCounterClockwise = RasterizerState.CullCounterClockwise.Clone();
            _rasterizerStateCullNone = RasterizerState.CullNone.Clone();

            RasterizerState = RasterizerState.CullCounterClockwise;

            EffectCache = new Dictionary<int, Effect>();
        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        internal int GetClampedMultisampleCount(int multiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                var msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);
                // and clamp it to what the device can handle
                if (msc > GraphicsCapabilities.MaxMultiSampleCount)
                    msc = GraphicsCapabilities.MaxMultiSampleCount;

                return msc;
            }
            else return 0;
        }

        internal void Initialize()
        {
            PlatformInitialize();

            // Force set the default render states.
            _blendStateDirty = _depthStencilStateDirty = _rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Force set the buffers and shaders on next ApplyState() call
            _vertexBuffers = new VertexBufferBindings(_maxVertexBufferSlots);
            _vertexBuffersDirty = true;
            _indexBufferDirty = true;

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets(null);
        }

        public RasterizerState RasterizerState
        {
            get => _rasterizerState;

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_rasterizerState == value)
                    return;

                if (!value.DepthClipEnable && !GraphicsCapabilities.SupportsDepthClamp)
                    throw new InvalidOperationException("Cannot set RasterizerState.DepthClipEnable to false on this graphics device");

                _rasterizerState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newRasterizerState = _rasterizerState;
                if (ReferenceEquals(_rasterizerState, RasterizerState.CullClockwise))
                    newRasterizerState = _rasterizerStateCullClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullCounterClockwise))
                    newRasterizerState = _rasterizerStateCullCounterClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullNone))
                    newRasterizerState = _rasterizerStateCullNone;

                newRasterizerState.BindToGraphicsDevice(this);

                _actualRasterizerState = newRasterizerState;

                _rasterizerStateDirty = true;
            }
        }

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// When only changing BlendFactor, use this rather than <see cref="Graphics.BlendState.BlendFactor"/> to
        /// only update BlendFactor so the whole BlendState does not have to be updated.
        /// </remarks>
        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (_blendFactor == value)
                    return;
                _blendFactor = value;
            }
        }

        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_blendState == value)
                    return;

                _blendState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newBlendState = _blendState;
                if (ReferenceEquals(_blendState, BlendState.Additive))
                    newBlendState = _blendStateAdditive;
                else if (ReferenceEquals(_blendState, BlendState.AlphaBlend))
                    newBlendState = _blendStateAlphaBlend;
                else if (ReferenceEquals(_blendState, BlendState.NonPremultiplied))
                    newBlendState = _blendStateNonPremultiplied;
                else if (ReferenceEquals(_blendState, BlendState.Opaque))
                    newBlendState = _blendStateOpaque;

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(this);

                _actualBlendState = newBlendState;

                BlendFactor = _actualBlendState.BlendFactor;

                _blendStateDirty = true;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get { return _depthStencilState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_depthStencilState == value)
                    return;

                _depthStencilState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newDepthStencilState = _depthStencilState;
                if (ReferenceEquals(_depthStencilState, DepthStencilState.Default))
                    newDepthStencilState = _depthStencilStateDefault;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.DepthRead))
                    newDepthStencilState = _depthStencilStateDepthRead;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.None))
                    newDepthStencilState = _depthStencilStateNone;

                newDepthStencilState.BindToGraphicsDevice(this);

                _actualDepthStencilState = newDepthStencilState;

                _depthStencilStateDirty = true;
            }
        }

        internal void ApplyState(bool applyShaders)
        {
            Threading.EnsureUIThread();

            PlatformApplyBlend();

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            PlatformApplyState(applyShaders);
        }

        public void Clear(Vector4 color)
        {
            var options = ClearOptions.Target;
            options |= ClearOptions.DepthBuffer;
            options |= ClearOptions.Stencil;
            PlatformClear(options, color, _viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil) =>
            PlatformClear(options, color, depth, stencil);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose of all remaining graphics resources before disposing of the graphics device
                    lock (_resourcesLock)
                    {
                        foreach (var resource in _resources.ToArray())
                        {
                            var target = resource.Target as IDisposable;
                            if (target != null)
                                target.Dispose();
                        }
                        _resources.Clear();
                    }

                    // Clear the effect cache.
                    EffectCache.Clear();

                    _blendState = null;
                    _actualBlendState = null;
                    _blendStateAdditive.Dispose();
                    _blendStateAlphaBlend.Dispose();
                    _blendStateNonPremultiplied.Dispose();
                    _blendStateOpaque.Dispose();

                    _depthStencilState = null;
                    _actualDepthStencilState = null;
                    _depthStencilStateDefault.Dispose();
                    _depthStencilStateDepthRead.Dispose();
                    _depthStencilStateNone.Dispose();

                    _rasterizerState = null;
                    _actualRasterizerState = null;
                    _rasterizerStateCullClockwise.Dispose();
                    _rasterizerStateCullCounterClockwise.Dispose();
                    _rasterizerStateCullNone.Dispose();

                    PlatformDispose();
                }

                _isDisposed = true;
            }
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }

        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (_currentRenderTargetCount != 0)
                return;

            PlatformPresent();
        }

        partial void PlatformReset();

        public void Reset()
        {
            PlatformReset();
            OnPresentationChanged();
            PresentationChanged?.Invoke(this, presentationParameters);
        }

        public void Reset(PresentationParameters presentationParameters)
        {
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            this.presentationParameters = presentationParameters;
            Reset();
        }

        public Viewport Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                PlatformSetViewport(ref value);
            }
        }

        public Rectangle ScissorRectangle
        {
            get => _scissorRectangle;
            set
            {
                if (_scissorRectangle == value)
                    return;

                _scissorRectangle = value;
                _scissorRectangleDirty = true;
            }
        }

        public int RenderTargetCount => _currentRenderTargetCount;

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        public void SetRenderTarget(RenderTarget2D renderTarget, int mipLevel)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, mipLevel);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace, int mipLevel)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace, mipLevel);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
            // Avoid having to check for null and zero length.
            var renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                {
                    renderTargets = null;
                }
            }

            // Try to early out if the current and new bindings are equal.
            if (_currentRenderTargetCount == renderTargetCount)
            {
                var isEqual = true;
                for (var i = 0; i < _currentRenderTargetCount; i++)
                {
                    if (_currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        _currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice ||
                        _currentRenderTargetBindings[i].MipLevel != renderTargets[i].MipLevel)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            ApplyRenderTargets(renderTargets);
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            var clearTarget = false;

            PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                _currentRenderTargetCount = 0;

                PlatformApplyDefaultRenderTarget();
                clearTarget = PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
            }
            else
            {
                // Copy the new bindings.
                Array.Copy(renderTargets, _currentRenderTargetBindings, renderTargets.Length);
                _currentRenderTargetCount = renderTargets.Length;

                var renderTarget = PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            // In XNA 4, because of hardware limitations on Xbox, when
            // a render target doesn't have PreserveContents as its usage
            // it is cleared before being rendered to.
            if (clearTarget)
                Clear(DiscardDefault);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffersDirty |= (vertexBuffer == null)
                                   ? _vertexBuffers.Clear()
                                   : _vertexBuffers.Set(vertexBuffer, 0);
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
        {
            // Validate vertexOffset.
            if (vertexOffset < 0
                || vertexBuffer == null && vertexOffset != 0
                || vertexBuffer != null && vertexOffset >= vertexBuffer.VertexCount)
            {
                throw new ArgumentOutOfRangeException("vertexOffset");
            }

            _vertexBuffersDirty |= (vertexBuffer == null)
                                   ? _vertexBuffers.Clear()
                                   : _vertexBuffers.Set(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
        {
            if (vertexBuffers == null || vertexBuffers.Length == 0)
            {
                _vertexBuffersDirty |= _vertexBuffers.Clear();
            }
            else
            {
                if (vertexBuffers.Length > _maxVertexBufferSlots)
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "Max number of vertex buffers is {0}.", _maxVertexBufferSlots);
                    throw new ArgumentOutOfRangeException("vertexBuffers", message);
                }

                _vertexBuffersDirty |= _vertexBuffers.Set(vertexBuffers);
            }
        }

        public void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            if (_indexBuffer == indexBuffer)
                return;

            _indexBuffer = indexBuffer;
            _indexBufferDirty = true;
        }

        internal VertexShader VertexShader
        {
            get => _vertexShader;

            set
            {
                if (_vertexShader == value)
                    return;

                _vertexShader = value;
            }
        }

        internal VertexShader TessellationControlShader
        {
            get => _tessellationControlShader;

            set
            {
                if (_tessellationControlShader == value)
                    return;

                _tessellationControlShader = value;
            }
        }

        internal VertexShader TessellationEvaluationShader
        {
            get => _tessellationEvaluationShader;

            set
            {
                if (_tessellationEvaluationShader == value)
                    return;

                _tessellationEvaluationShader = value;
            }
        }

        internal PixelShader PixelShader
        {
            get => _pixelShader;

            set
            {
                if (_pixelShader == value)
                    return;

                _pixelShader = value;
            }
        }

        public void DrawIndexedPrimitives(GLPrimitiveType primitiveType, int baseVertex, int startIndex, int indexCount)
        {
            ApplyState(true);

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

            var indexElementType = shortIndices ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
            var indexElementSize = shortIndices ? 2 : 4;
            var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);

            ApplyAttribs(_vertexShader, baseVertex);

            GL.DrawElements(primitiveType, indexCount, indexElementType, indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();
        }

        public void DrawInstancedPrimitives(GLPrimitiveType primitiveType, int baseVertex, int startIndex, int indexCount, int baseInstance, int instanceCount)
        {
            if (!GraphicsCapabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");
            ApplyState(true);

            var shortIndices = _indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

            var indexElementType = shortIndices ? DrawElementsType.UnsignedShort : DrawElementsType.UnsignedInt;
            var indexElementSize = shortIndices ? 2 : 4;
            var indexOffsetInBytes = (IntPtr)(startIndex * indexElementSize);

            ApplyAttribs(_vertexShader, baseVertex);

            if (baseInstance > 0)
            {
                if (!GraphicsCapabilities.SupportsBaseIndexInstancing)
                    throw new PlatformNotSupportedException("Instanced geometry drawing with base instance requires at least OpenGL 4.2. Try upgrading your graphics card drivers.");

                GL.DrawElementsInstancedBaseInstance(primitiveType, indexCount, indexElementType, indexOffsetInBytes, instanceCount, baseInstance);
            }
            else
                GL.DrawElementsInstanced(primitiveType, indexCount, indexElementType, indexOffsetInBytes, instanceCount);

            GraphicsExtensions.CheckGLError();
        }

        public void DrawUserPrimitives<T>(GLPrimitiveType primitiveType, T[] vertexData, int vertexOffset, int indexCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vbHandle.AddrOfPinnedObject(), ShaderProgramHash);

            //Draw
            GL.DrawArrays(primitiveType,
                          vertexOffset,
                          indexCount);
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            vbHandle.Free();
        }

        public void DrawUserIndexedPrimitives<T>(GLPrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int indexCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vertexAddr, ShaderProgramHash);

            //Draw
            GL.DrawElements(primitiveType,
                                indexCount,
                                DrawElementsType.UnsignedShort,
                                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();
        }

        public void DrawUserIndexedPrimitives<T>(GLPrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int indexCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vertexAddr, ShaderProgramHash);

            //Draw
            GL.DrawElements(primitiveType,
                                indexCount,
                                DrawElementsType.UnsignedInt,
                                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(int))));
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();
        }
    }
}
