// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.OpenGL
{
    public enum BufferAccess
    {
        ReadOnly = 0x88B8,
    }

    public enum BufferUsageHint
    {
        StreamDraw = 0x88E0,
        StaticDraw = 0x88E4,
    }

    public enum StencilFace
    {
        Front = 0x0404,
        Back = 0x0405,
    }
    public enum DrawBuffersEnum
    {
        UnsignedShort,
        UnsignedInt,
    }

    public enum ShaderType
    {
        VertexShader = 0x8B31,
        FragmentShader = 0x8B30,
        TessellationControlShader = 0x8e88,
        TessellationEvaluationShader = 0x8e87
    }

    public enum ShaderParameter
    {
        LogLength = 0x8B84,
        CompileStatus = 0x8B81,
        SourceLength = 0x8B88,
    }

    public enum GetProgramParameterName
    {
        DeleteStatus = 0x8B80,
        LinkStatus = 0x8B82,
        ValidateStatus = 0x8B83,
        LogLength = 0x8B84,
        AttachedShaders = 0x8B85,
        ActiveUniforms = 0x8B86,
        ActiveuniformMaxLength = 0x8B87,
        ActiveAttributes = 0x8B89,
        ActiveAttributeMaxLength = 0x8B8A,
    }

    public enum ProgramInterfaceName
    {
        ActiveResources = 0x92F5,
        MaxNameLength = 0x92F6,
        MaxNumActiveVariables = 0x92F7,
        MaxNumCompatibleSubroutines = 0x92F8
    }

    public enum ProgramInterface
    {
        Uniform = 0x92E1,
        UniformBlock = 0x92E2,
        //#define GL_ATOMIC_COUNTER_BUFFER_INDEX    0x9301
        ProgramInput = 0x92E3,
        ProgramOutput = 0x92E4,
        //#define GL_VERTEX_SUBROUTINE              0x92E8
        //#define GL_TESS_CONTROL_SUBROUTINE        0x92E9
        //#define GL_TESS_EVALUATION_SUBROUTINE     0x92EA
        //#define GL_GEOMETRY_SUBROUTINE            0x92EB
        //#define GL_FRAGMENT_SUBROUTINE            0x92EC
        //#define GL_COMPUTE_SUBROUTINE             0x92ED
        //#define GL_TESS_CONTROL_SUBROUTINE_UNIFORM 0x92EF
        //#define GL_TESS_EVALUATION_SUBROUTINE_UNIFORM 0x92F0
        //#define GL_GEOMETRY_SUBROUTINE_UNIFORM    0x92F1
        //#define GL_FRAGMENT_SUBROUTINE_UNIFORM    0x92F2
        //#define GL_COMPUTE_SUBROUTINE_UNIFORM     0x92F3
        //#define GL_TRANSFORM_FEEDBACK_VARYING     0x92F4
        BufferVariable = 0x92E5,
        ShaderStorageBlock = 0x92E6,
        //#define GL_TRANSFORM_FEEDBACK_BUFFER      0x8C8E
    }

    public enum ProgramResourceProperties
    {
        NameLength = 0x92F9,
        Type = 0x92FA,
        ArraySize = 0x92FB,
        Offset = 0x92FC,
        BlockIndex = 0x92FD,
        ArrayStride = 0x92FE,
        MatrixStride = 0x92FF,
        IsRowMajor = 0x9300,
        //#define GL_ATOMIC_COUNTER_BUFFER_INDEX    0x9301
        //#define GL_TEXTURE_BUFFER                 0x8C2A
        //#define GL_BUFFER_BINDING                 0x9302
        //#define GL_BUFFER_DATA_SIZE               0x9303
        //#define GL_NUM_ACTIVE_VARIABLES           0x9304
        //#define GL_ACTIVE_VARIABLES               0x9305
        //#define GL_REFERENCED_BY_VERTEX_SHADER    0x9306
        //#define GL_REFERENCED_BY_TESS_CONTROL_SHADER 0x9307
        //#define GL_REFERENCED_BY_TESS_EVALUATION_SHADER 0x9308
        //#define GL_REFERENCED_BY_GEOMETRY_SHADER  0x9309
        //#define GL_REFERENCED_BY_FRAGMENT_SHADER  0x930A
        //#define GL_REFERENCED_BY_COMPUTE_SHADER   0x930B
        //#define GL_NUM_COMPATIBLE_SUBROUTINES     0x8E4A
        //#define GL_COMPATIBLE_SUBROUTINES         0x8E4B
        //#define GL_TOP_LEVEL_ARRAY_SIZE           0x930C
        //#define GL_TOP_LEVEL_ARRAY_STRIDE         0x930D
        Location = 0x930E,
        LocationIndex = 0x930F
        //#define GL_IS_PER_PATCH                   0x92E7
        //#define GL_LOCATION_COMPONENT             0x934A
        //#define GL_TRANSFORM_FEEDBACK_BUFFER_INDEX 0x934B
        //#define GL_TRANSFORM_FEEDBACK_BUFFER_STRIDE 0x934C
    }

    public enum ProgramResourceType
    {
        Float = 0x1406,
        Vec2 = 0x8B50,
        Vec3 = 0x8B51,
        Vec4 = 0x8B52,
        Double = 0x140A,
        DVec2 = 0x8FFC,
        DVec3 = 0x8FFD,
        DVec4 = 0x8FFE,
        Int = 0x1404,
        IVec2 = 0x8B53,
        IVec3 = 0x8B54,
        IVec4 = 0x8B55,
        UInt = 0x1405,
        UVec2 = 0x8DC6,
        UVec3 = 0x8DC7,
        UVec4 = 0x8DC8,
        Bool = 0x8B56,
        BVec2 = 0x8B57,
        BVec3 = 0x8B58,
        BVec4 = 0x8B59,
        Mat2 = 0x8B5A,
        Mat3 = 0x8B5B,
        Mat4 = 0x8B5C,
        Mat2x3 = 0x8B65,
        Mat2x4 = 0x8B66,
        Mat3x2 = 0x8B67,
        Mat3x4 = 0x8B68,
        GMat4x2 = 0x8B69,
        Mat4x3 = 0x8B6A,
        DMat2 = 0x8F46,
        DMat3 = 0x8F47,
        DMat4 = 0x8F48,
        DMat2x3 = 0x8F49,
        DMat2x4 = 0x8F4A,
        DMat3x2 = 0x8F4B,
        DMat3x4 = 0x8F4C,
        DMat4x2 = 0x8F4D,
        DMat4x3 = 0x8F4E,
        Sampler1d = 0x8B5D,
        Sampler2d = 0x8B5E,
        Sampler3d = 0x8B5F,
        SamplerCube = 0x8B60,
        Sampler1dShadow = 0x8B61,
        Sampler2dShadow = 0x8B62,
        Sampler1dArray = 0x8DC0,
        Sampler2dArray = 0x8DC1,
        Sampler1dArrayShadow = 0x8DC3,
        Sampler2dArrayShadow = 0x8DC4,
        Sampler2dMs = 0x9108,
        Sampler2dMsArray = 0x910B,
        SamplerCubeShadow = 0x8DC5,
        SamplerBuffer = 0x8DC2,
        Sampler2dRect = 0x8B63,
        Sampler2dRectShadow = 0x8B64,
        ISampler1d = 0x8DC9,
        ISampler2d = 0x8DCA,
        ISampler3d = 0x8DCB,
        ISamplerCube = 0x8DCC,
        ISampler1dArray = 0x8DCE,
        ISampler2dArray = 0x8DCF,
        ISampler2dMs = 0x9109,
        ISampler2dMsArray = 0x910C,
        ISamplerBuffer = 0x8DD0,
        ISampler2dRect = 0x8DCD,
        USampler1d = 0x8DD1,
        USampler2d = 0x8DD2,
        USampler3d = 0x8DD3,
        USamplerCube = 0x8DD4,
        USampler1dArray = 0x8DD6,
        USampler2dArray = 0x8DD7,
        USampler2dMs = 0x910A,
        USampler2dMsArray = 0x910D,
        USamplerBuffer = 0x8DD8,
        USampler2dRect = 0x8DD5
    }

    public enum DrawElementsType
    {
        UnsignedShort = 0x1403,
        UnsignedInt = 0x1405,
    }

    public enum QueryTarget
    {
        SamplesPassed = 0x8914,
        SamplesPassedExt = 0x8C2F,
    }

    public enum GetQueryObjectParam
    {
        QueryResultAvailable = 0x8867,
        QueryResult = 0x8866,
    }

    public enum GenerateMipmapTarget
    {
        Texture1D = 0x0DE0,
        Texture2D = 0x0DE1,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        Texture1DArray = 0x8C18,
        Texture2DArray = 0x8C1A,
        Texture2DMultisample = 0x9100,
        Texture2DMultisampleArray = 0x9102,
    }

    public enum BlitFramebufferFilter
    {
        Nearest = 0x2600,
    }

    public enum ReadBufferMode
    {
        ColorAttachment0 = 0x8CE0,
    }

    public enum DrawBufferMode
    {
        ColorAttachment0 = 0x8CE0,
    }

    public enum FramebufferErrorCode
    {
        FramebufferUndefined = 0x8219,
        FramebufferComplete = 0x8CD5,
        FramebufferCompleteExt = 0x8CD5,
        FramebufferIncompleteAttachment = 0x8CD6,
        FramebufferIncompleteAttachmentExt = 0x8CD6,
        FramebufferIncompleteMissingAttachment = 0x8CD7,
        FramebufferIncompleteMissingAttachmentExt = 0x8CD7,
        FramebufferIncompleteDimensionsExt = 0x8CD9,
        FramebufferIncompleteFormatsExt = 0x8CDA,
        FramebufferIncompleteDrawBuffer = 0x8CDB,
        FramebufferIncompleteDrawBufferExt = 0x8CDB,
        FramebufferIncompleteReadBuffer = 0x8CDC,
        FramebufferIncompleteReadBufferExt = 0x8CDC,
        FramebufferUnsupported = 0x8CDD,
        FramebufferUnsupportedExt = 0x8CDD,
        FramebufferIncompleteMultisample = 0x8D56,
        FramebufferIncompleteLayerTargets = 0x8DA8,
        FramebufferIncompleteLayerCount = 0x8DA9,
    }

    public enum BufferTarget
    {
        ArrayBuffer = 0x8892,
        ElementArrayBuffer = 0x8893,
        UniformBuffer = 0x8A11
    }

    public enum RenderbufferTarget
    {
        Renderbuffer = 0x8D41,
        RenderbufferExt = 0x8D41,
    }

    public enum FramebufferTarget
    {
        Framebuffer = 0x8D40,
        FramebufferExt = 0x8D40,
        ReadFramebuffer = 0x8CA8,
    }

    public enum RenderbufferStorage
    {
        Rgba8 = 0x8058,
        DepthComponent16 = 0x81A5,
        DepthComponent24 = 0x81A6,
        DepthComponent32 = 0x81A7,
        DepthComponent32F = 0x8CAC,
        Depth24Stencil8 = 0x88F0,
        Depth32FStencil8 = 0x8CAD
    }

    public enum EnableCap : int
    {
        PointSmooth = 0x0B10,
        LineSmooth = 0x0B20,
        CullFace = 0x0B44,
        Lighting = 0x0B50,
        ColorMaterial = 0x0B57,
        Fog = 0x0B60,
        DepthTest = 0x0B71,
        StencilTest = 0x0B90,
        Normalize = 0x0BA1,
        AlphaTest = 0x0BC0,
        Dither = 0x0BD0,
        Blend = 0x0BE2,
        ColorLogicOp = 0x0BF2,
        ScissorTest = 0x0C11,
        Texture2D = 0x0DE1,
        PolygonOffsetFill = 0x8037,
        RescaleNormal = 0x803A,
        VertexArray = 0x8074,
        NormalArray = 0x8075,
        ColorArray = 0x8076,
        TextureCoordArray = 0x8078,
        Multisample = 0x809D,
        SampleAlphaToCoverage = 0x809E,
        SampleAlphaToOne = 0x809F,
        SampleCoverage = 0x80A0,
        DebugOutputSynchronous = 0x8242,
        DebugOutput = 0x92E0,
        TextureCubeMapSeamless = 0x884f,
        DepthClamp = 0x864f
    }

    public enum VertexPointerType
    {
        Float = 0x1406,
        Short = 0x1402,
    }

    public enum VertexAttribPointerType
    {
        Float = 0x1406,
        Short = 0x1402,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum CullFaceMode
    {
        Back = 0x0405,
        Front = 0x0404,
    }

    public enum FrontFaceDirection
    {
        Cw = 0x0900,
        Ccw = 0x0901,
    }

    public enum MaterialFace
    {
        FrontAndBack = 0x0408,
    }

    public enum PolygonMode
    {
        Fill = 0x1B02,
        Line = 0x1B01,
    }

    public enum ColorPointerType
    {
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum NormalPointerType
    {
        Byte = 0x1400,
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum TexCoordPointerType
    {
        Byte = 0x1400,
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum BlendEquationMode
    {
        FuncAdd = 0x8006,
        Max = 0x8008,  // ios MaxExt
        Min = 0x8007,  // ios MinExt
        FuncReverseSubtract = 0x800B,
        FuncSubtract = 0x800A,
    }

    public enum BlendingFactorSrc
    {
        Zero = 0,
        SrcColor = 0x0300,
        OneMinusSrcColor = 0x0301,
        SrcAlpha = 0x0302,
        OneMinusSrcAlpha = 0x0303,
        DstAlpha = 0x0304,
        OneMinusDstAlpha = 0x0305,
        DstColor = 0x0306,
        OneMinusDstColor = 0x0307,
        SrcAlphaSaturate = 0x0308,
        ConstantColor = 0x8001,
        OneMinusConstantColor = 0x8002,
        ConstantAlpha = 0x8003,
        OneMinusConstantAlpha = 0x8004,
        One = 1,
    }

    public enum BlendingFactorDest
    {
        Zero = 0,
        SrcColor = 0x0300,
        OneMinusSrcColor = 0x0301,
        SrcAlpha = 0x0302,
        OneMinusSrcAlpha = 0x0303,
        DstAlpha = 0x0304,
        OneMinusDstAlpha = 0x0305,
        DstColor = 0X0306,
        OneMinusDstColor = 0x0307,
        SrcAlphaSaturate = 0x0308,
        ConstantColor = 0x8001,
        OneMinusConstantColor = 0x8002,
        ConstantAlpha = 0x8003,
        OneMinusConstantAlpha = 0x8004,
        One = 1,
    }

    public enum DepthFunction
    {
        Always = 0x0207,
        Equal = 0x0202,
        Greater = 0x0204,
        Gequal = 0x0206,
        Less = 0x0201,
        Lequal = 0x0203,
        Never = 0x0200,
        Notequal = 0x0205,
    }

    public enum GetPName : int
    {
        ArrayBufferBinding = 0x8894,
        MaxTextureImageUnits = 0x8872,
        MaxVertexAttribs = 0x8869,
        MaxTextureSize = 0x0D33,
        MaxDrawBuffers = 0x8824,
        TextureBinding2D = 0x8069,
        MaxTextureMaxAnisotropyExt = 0x84FF,
        MaxSamples = 0x8D57,
    }

    public enum StringName
    {
        Extensions = 0x1F03,
        Version = 0x1F02,
        ShaderLanguageVersion = 0x8B8C
    }

    public enum FramebufferAttachment
    {
        ColorAttachment0 = 0x8CE0,
        ColorAttachment0Ext = 0x8CE0,
        DepthAttachment = 0x8D00,
        StencilAttachment = 0x8D20,
        ColorAttachmentExt = 0x1800,
        DepthAttachementExt = 0x1801,
        StencilAttachmentExt = 0x1802,
    }

    public enum GLPrimitiveType
    {
        Lines = 0x0001,
        LineStrip = 0x0003,
        Triangles = 0x0004,
        TriangleStrip = 0x0005,
        Patches = 	0x000E
    }

    [Flags]
    public enum ClearBufferMask
    {
        DepthBufferBit = 0x00000100,
        StencilBufferBit = 0x00000400,
        ColorBufferBit = 0x00004000,
    }

    public enum ErrorCode
    {
        NoError = 0,
    }

    public enum TextureUnit
    {
        Texture0 = 0x84C0,
    }

    public enum TextureTarget
    {
        Texture2D = 0x0DE1,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        TextureCubeMapPositiveX = 0x8515,
        TextureCubeMapPositiveY = 0x8517,
        TextureCubeMapPositiveZ = 0x8519,
        TextureCubeMapNegativeX = 0x8516,
        TextureCubeMapNegativeY = 0x8518,
        TextureCubeMapNegativeZ = 0x851A,
    }

    public enum PixelInternalFormat
    {
        Red = 0x1903,
        Rg = 0x8227,
        Rgb = 0x1907,
        Rgba = 0x1908,

        Srgb = 0x8C40,

        R16f = 0x822D,
        R32f = 0x822E,

        Rg16f = 0x822F,
        Rg32f = 0x8230,

        Rgb16f = 0x881B,
        Rgb32f = 0x8815,

        Rgba16f = 0x881A,
        Rgba32f = 0x8814,

        Rg16 = 0x8054,
        Rgba16 = 0x805B,
    }

    public enum PixelFormat
    {
        Rgba = 0x1908,
        Rgb = 0x1907,
        Luminance = 0x1909,
        CompressedTextureFormats = 0x86A3,
        Red = 0x1903,
        Rg = 0x8227,
    }

    public enum PixelType
    {
        UnsignedByte = 0x1401,
        UnsignedShort565 = 0x8363,
        UnsignedShort4444 = 0x8033,
        UnsignedShort5551 = 0x8034,
        Float = 0x1406,
        HalfFloat = 0x140B,
        HalfFloatOES = 0x8D61,
        Byte = 0x1400,
        UnsignedShort = 0x1403,
        UnsignedInt1010102 = 0x8036,
    }

    public enum PixelStoreParameter
    {
        UnpackAlignment = 0x0CF5,
        PackAlignment = 0x0D05,
    }

    public enum GLStencilFunction
    {
        Always = 0x0207,
        Equal = 0x0202,
        Greater = 0x0204,
        Gequal = 0x0206,
        Less = 0x0201,
        Lequal = 0x0203,
        Never = 0x0200,
        Notequal = 0x0205,
    }

    public enum StencilOp
    {
        Keep = 0x1E00,
        DecrWrap = 0x8508,
        Decr = 0x1E03,
        Incr = 0x1E02,
        IncrWrap = 0x8507,
        Invert = 0x150A,
        Replace = 0x1E01,
        Zero = 0,
    }

    public enum TextureParameterName
    {
        TextureMaxAnisotropyExt = 0x84FE,
        TextureBaseLevel = 0x813C,
        TextureMaxLevel = 0x813D,
        TextureMinFilter = 0x2801,
        TextureMagFilter = 0x2800,
        TextureWrapS = 0x2802,
        TextureWrapT = 0x2803,
        TextureWrapR = 0x8072,
        TextureBorderColor = 0x1004,
        TextureLodBias = 0x8501,
        TextureCompareMode = 0x884C,
        TextureCompareFunc = 0x884D,
        GenerateMipmap = 0x8191,
    }

    public enum Bool
    {
        True = 1,
        False = 0,
    }

    public enum TextureMinFilter
    {
        LinearMipmapNearest = 0x2701,
        NearestMipmapLinear = 0x2702,
        LinearMipmapLinear = 0x2703,
        Linear = 0x2601,
        NearestMipmapNearest = 0x2700,
        Nearest = 0x2600,
    }

    public enum TextureMagFilter
    {
        Linear = 0x2601,
        Nearest = 0x2600,
    }

    public enum TextureCompareMode
    {
        CompareRefToTexture = 0x884E,
        None = 0,
    }

    public enum TextureWrapMode
    {
        ClampToEdge = 0x812F,
        Repeat = 0x2901,
        MirroredRepeat = 0x8370,
        //GLES
        ClampToBorder = 0x812D,
    }

    public enum GLPatchParameter
    {
        PatchVertices = 0x8E72,
        DefaultInnerLevel = 0x8E73,
        DefaultOuterLevel = 0x8E74,
        MaxPatchVertices = 0x8E7D,
        MaxTessGenLevel = 0x8E7E
    }

    internal partial class ColorFormat
    {
        internal ColorFormat(int r, int g, int b, int a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        internal int R { get; private set; }
        internal int G { get; private set; }
        internal int B { get; private set; }
        internal int A { get; private set; }
    }

    public partial class GL
    {
        public enum RenderApi
        {
            ES = 12448,
            GL = 12450,
        }

        public static RenderApi BoundApi = RenderApi.GL;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void EnableVertexAttribArrayDelegate(int attrib);
        public static EnableVertexAttribArrayDelegate EnableVertexAttribArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DisableVertexAttribArrayDelegate(int attrib);
        public static DisableVertexAttribArrayDelegate DisableVertexAttribArray;

        //[System.Security.SuppressUnmanagedCodeSecurity ()]
        //[MonoNativeFunctionWrapper]
        //public delegate void MakeCurrentDelegate (IntPtr window);
        //public static MakeCurrentDelegate MakeCurrent;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void GetIntegerDelegate(int param, [Out] int* data);
        public static GetIntegerDelegate GetIntegerv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate IntPtr GetStringDelegate(StringName param);
        public static GetStringDelegate GetStringInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ClearDepthDelegate(float depth);
        public static ClearDepthDelegate ClearDepth;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DepthRangedDelegate(double min, double max);
        public static DepthRangedDelegate DepthRanged;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DepthRangefDelegate(float min, float max);
        public static DepthRangefDelegate DepthRangef;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ClearDelegate(ClearBufferMask mask);
        public static ClearDelegate Clear;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ClearColorDelegate(float red, float green, float blue, float alpha);
        public static ClearColorDelegate ClearColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ClearStencilDelegate(int stencil);
        public static ClearStencilDelegate ClearStencil;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ViewportDelegate(int x, int y, int w, int h);
        public static ViewportDelegate Viewport;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate ErrorCode GetErrorDelegate();
        public static GetErrorDelegate GetError;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FlushDelegate();
        public static FlushDelegate Flush;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenTexturesDelegte(int count, [Out] out int id);
        public static GenTexturesDelegte GenTextures;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BindTextureDelegate(TextureTarget target, int id);
        public static BindTextureDelegate BindTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int EnableDelegate(EnableCap cap);
        public static EnableDelegate Enable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int DisableDelegate(EnableCap cap);
        public static DisableDelegate Disable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void CullFaceDelegate(CullFaceMode mode);
        public static CullFaceDelegate CullFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FrontFaceDelegate(FrontFaceDirection direction);
        public static FrontFaceDelegate FrontFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void PolygonModeDelegate(MaterialFace face, PolygonMode mode);
        public static PolygonModeDelegate PolygonMode;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void PolygonOffsetDelegate(float slopeScaleDepthBias, float depthbias);
        public static PolygonOffsetDelegate PolygonOffset;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DrawBuffersDelegate(int count, DrawBuffersEnum[] buffers);
        public static DrawBuffersDelegate DrawBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void UseProgramDelegate(int program);
        public static UseProgramDelegate UseProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ScissorDelegate(int x, int y, int width, int height);
        public static ScissorDelegate Scissor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ReadPixelsDelegate(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr data);
        public static ReadPixelsDelegate ReadPixelsInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BindBufferDelegate(BufferTarget target, int buffer);
        public static BindBufferDelegate BindBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DrawElementsDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType, IntPtr offset);
        public static DrawElementsDelegate DrawElements;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DrawArraysDelegate(GLPrimitiveType primitiveType, int offset, int count);
        public static DrawArraysDelegate DrawArrays;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenRenderbuffersDelegate(int count, [Out] out int buffer);
        public static GenRenderbuffersDelegate GenRenderbuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BindRenderbufferDelegate(RenderbufferTarget target, int buffer);
        public static BindRenderbufferDelegate BindRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteRenderbuffersDelegate(int count, [In][Out] ref int buffer);
        public static DeleteRenderbuffersDelegate DeleteRenderbuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void RenderbufferStorageMultisampleDelegate(RenderbufferTarget target, int sampleCount,
            RenderbufferStorage storage, int width, int height);
        public static RenderbufferStorageMultisampleDelegate RenderbufferStorageMultisample;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void Uniform1iDelegate(int location, int value);
        public static Uniform1iDelegate Uniform1i;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform1ivDelegate(int location, int size, byte* values);
        public static Uniform1ivDelegate Uniform1iv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform2ivDelegate(int location, int size, byte* values);
        public static Uniform2ivDelegate Uniform2iv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform3ivDelegate(int location, int size, byte* values);
        public static Uniform3ivDelegate Uniform3iv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform4ivDelegate(int location, int size, byte* values);
        public static Uniform4ivDelegate Uniform4iv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform1fvDelegate(int location, int size, byte* values);
        public static Uniform1fvDelegate Uniform1fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform2fvDelegate(int location, int size, byte* values);
        public static Uniform2fvDelegate Uniform2fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform3fvDelegate(int location, int size, byte* values);
        public static Uniform3fvDelegate Uniform3fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void Uniform4fvDelegate(int location, int size, byte* values);
        public static Uniform4fvDelegate Uniform4fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void UniformMatrix2fvDelegate(int location, int size, bool transpose, byte* values);
        public static UniformMatrix2fvDelegate UniformMatrix2fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void UniformMatrix3fvDelegate(int location, int size, bool transpose, byte* values);
        public static UniformMatrix3fvDelegate UniformMatrix3fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void UniformMatrix4fvDelegate(int location, int size, bool transpose, byte* values);
        public static UniformMatrix4fvDelegate UniformMatrix4fv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenFramebuffersDelegate(int count, out int buffer);
        public static GenFramebuffersDelegate GenFramebuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BindFramebufferDelegate(FramebufferTarget target, int buffer);
        public static BindFramebufferDelegate BindFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteFramebuffersDelegate(int count, ref int buffer);
        public static DeleteFramebuffersDelegate DeleteFramebuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void InvalidateFramebufferDelegate(FramebufferTarget target, int numAttachments, FramebufferAttachment[] attachments);
        public static InvalidateFramebufferDelegate InvalidateFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FramebufferTexture2DDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            TextureTarget textureTarget, int texture, int level);
        public static FramebufferTexture2DDelegate FramebufferTexture2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FramebufferTexture2DMultiSampleDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            TextureTarget textureTarget, int texture, int level, int samples);
        public static FramebufferTexture2DMultiSampleDelegate FramebufferTexture2DMultiSample;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FramebufferRenderbufferDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            RenderbufferTarget renderBufferTarget, int buffer);
        public static FramebufferRenderbufferDelegate FramebufferRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void RenderbufferStorageDelegate(RenderbufferTarget target, RenderbufferStorage storage, int width, int hegiht);
        public static RenderbufferStorageDelegate RenderbufferStorage;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenerateMipmapDelegate(GenerateMipmapTarget target);
        public static GenerateMipmapDelegate GenerateMipmap;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ReadBufferDelegate(ReadBufferMode buffer);
        public static ReadBufferDelegate ReadBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DrawBufferDelegate(DrawBufferMode buffer);
        public static DrawBufferDelegate DrawBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BlitFramebufferDelegate(int srcX0,
            int srcY0,
            int srcX1,
            int srcY1,
            int dstX0,
            int dstY0,
            int dstX1,
            int dstY1,
            ClearBufferMask mask,
            BlitFramebufferFilter filter);
        public static BlitFramebufferDelegate BlitFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate FramebufferErrorCode CheckFramebufferStatusDelegate(FramebufferTarget target);
        public static CheckFramebufferStatusDelegate CheckFramebufferStatus;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexParameterFloatDelegate(TextureTarget target, TextureParameterName name, float value);
        public static TexParameterFloatDelegate TexParameterf;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void TexParameterFloatArrayDelegate(TextureTarget target, TextureParameterName name, float* values);
        public static TexParameterFloatArrayDelegate TexParameterfv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexParameterIntDelegate(TextureTarget target, TextureParameterName name, int value);
        public static TexParameterIntDelegate TexParameteri;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenQueriesDelegate(int count, [Out] out int queryId);
        public static GenQueriesDelegate GenQueries;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BeginQueryDelegate(QueryTarget target, int queryId);
        public static BeginQueryDelegate BeginQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void EndQueryDelegate(QueryTarget target);
        public static EndQueryDelegate EndQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GetQueryObjectDelegate(int queryId, GetQueryObjectParam getparam, [Out] out int ready);
        public static GetQueryObjectDelegate GetQueryObject;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteQueriesDelegate(int count, [In][Out] ref int queryId);
        public static DeleteQueriesDelegate DeleteQueries;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ActiveTextureDelegate(TextureUnit textureUnit);
        public static ActiveTextureDelegate ActiveTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int CreateShaderDelegate(ShaderType type);
        public static CreateShaderDelegate CreateShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void ShaderSourceDelegate(int shaderId, int count, IntPtr code, int* length);
        public static ShaderSourceDelegate ShaderSourceInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void CompileShaderDelegate(int shaderId);
        public static CompileShaderDelegate CompileShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void GetShaderDelegate(int shaderId, int parameter, int* value);
        public static GetShaderDelegate GetShaderiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void GetShaderInfoLogDelegate(int shader, int bufSize, IntPtr length, StringBuilder infoLog);
        public static GetShaderInfoLogDelegate GetShaderInfoLogInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate bool IsShaderDelegate(int shaderId);
        public static IsShaderDelegate IsShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteShaderDelegate(int shaderId);
        public static DeleteShaderDelegate DeleteShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int GetAttribLocationDelegate(int programId, string name);
        public static GetAttribLocationDelegate GetAttribLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int BindAttribLocationDelegate(int programId, int index, string name);
        public static BindAttribLocationDelegate BindAttribLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int GetUniformLocationDelegate(int programId, string name);
        public static GetUniformLocationDelegate GetUniformLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate int GetProgramInterfaceDelegate(int programId, int programInterface, int name, int* programParams);
        public static GetProgramInterfaceDelegate GetProgramInterfaceiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate int GetProgramResourceDelegate(int programId, int programInterface, int index, int propCount, int* props, int count, int* length, int* programParams);
        public static GetProgramResourceDelegate GetProgramResourceiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate int GetProgramResourceNameDelegate(int programId, int programInterface, int index, int count, int* length, StringBuilder name);
        public static GetProgramResourceNameDelegate GetProgramResourceNameInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate int GetUniformBlockIndexDelegate(int programId, string name);
        public static GetUniformBlockIndexDelegate GetUniformBlockIndex;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate int GetUniformIndicesDelegate(int programId, int uniformCount, string[] uniformNames, int* uniformIndices);
        public static GetUniformIndicesDelegate GetUniformIndicesInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate bool IsProgramDelegate(int programId);
        public static IsProgramDelegate IsProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteProgramDelegate(int programId);
        public static DeleteProgramDelegate DeleteProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate int CreateProgramDelegate();
        public static CreateProgramDelegate CreateProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void AttachShaderDelegate(int programId, int shaderId);
        public static AttachShaderDelegate AttachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void LinkProgramDelegate(int programId);
        public static LinkProgramDelegate LinkProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public unsafe delegate void GetProgramDelegate(int programId, int name, int* linked);
        public static GetProgramDelegate GetProgramiv;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GetProgramInfoLogDelegate(int program, int bufSize, IntPtr length, StringBuilder infoLog);
        public static GetProgramInfoLogDelegate GetProgramInfoLogInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DetachShaderDelegate(int programId, int shaderId);
        public static DetachShaderDelegate DetachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BlendColorDelegate(float r, float g, float b, float a);
        public static BlendColorDelegate BlendColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BlendEquationSeparateDelegate(BlendEquationMode colorMode, BlendEquationMode alphaMode);
        public static BlendEquationSeparateDelegate BlendEquationSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BlendFuncSeparateDelegate(BlendingFactorSrc colorSrc, BlendingFactorDest colorDst,
            BlendingFactorSrc alphaSrc, BlendingFactorDest alphaDst);
        public static BlendFuncSeparateDelegate BlendFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void ColorMaskDelegate(bool r, bool g, bool b, bool a);
        public static ColorMaskDelegate ColorMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DepthFuncDelegate(DepthFunction function);
        public static DepthFuncDelegate DepthFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DepthMaskDelegate(bool enabled);
        public static DepthMaskDelegate DepthMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void StencilFuncSeparateDelegate(StencilFace face, GLStencilFunction function, int referenceStencil, int mask);
        public static StencilFuncSeparateDelegate StencilFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void StencilOpSeparateDelegate(StencilFace face, StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        public static StencilOpSeparateDelegate StencilOpSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void StencilFuncDelegate(GLStencilFunction function, int referenceStencil, int mask);
        public static StencilFuncDelegate StencilFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void StencilOpDelegate(StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        public static StencilOpDelegate StencilOp;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void StencilMaskDelegate(int mask);
        public static StencilMaskDelegate StencilMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void CompressedTexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int border, int size, IntPtr data);
        public static CompressedTexImage2DDelegate CompressedTexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int border, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexImage2DDelegate TexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void CompressedTexSubImage2DDelegate(TextureTarget target, int level,
            int x, int y, int width, int height, PixelInternalFormat format, int size, IntPtr data);
        public static CompressedTexSubImage2DDelegate CompressedTexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexSubImage2DDelegate(TextureTarget target, int level,
            int x, int y, int width, int height, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexSubImage2DDelegate TexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void PixelStoreDelegate(PixelStoreParameter parameter, int size);
        public static PixelStoreDelegate PixelStore;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void FinishDelegate();
        public static FinishDelegate Finish;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GetTexImageDelegate(TextureTarget target, int level, PixelFormat format, PixelType type, [Out] IntPtr pixels);
        public static GetTexImageDelegate GetTexImageInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GetCompressedTexImageDelegate(TextureTarget target, int level, [Out] IntPtr pixels);
        public static GetCompressedTexImageDelegate GetCompressedTexImageInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexImage3DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int depth, int border, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexImage3DDelegate TexImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void TexSubImage3DDelegate(TextureTarget target, int level,
            int x, int y, int z, int width, int height, int depth, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexSubImage3DDelegate TexSubImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteTexturesDelegate(int count, ref int id);
        public static DeleteTexturesDelegate DeleteTextures;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void GenBuffersDelegate(int count, out int buffer);
        public static GenBuffersDelegate GenBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BufferDataDelegate(BufferTarget target, IntPtr size, IntPtr n, BufferUsageHint usage);
        public static BufferDataDelegate BufferData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate IntPtr MapBufferDelegate(BufferTarget target, BufferAccess access);
        public static MapBufferDelegate MapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void UnmapBufferDelegate(BufferTarget target);
        public static UnmapBufferDelegate UnmapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void BufferSubDataDelegate(BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);
        public static BufferSubDataDelegate BufferSubData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DeleteBuffersDelegate(int count, [In][Out] ref int buffer);
        public static DeleteBuffersDelegate DeleteBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void VertexAttribPointerDelegate(int location, int elementCount, VertexAttribPointerType type, bool normalize,
            int stride, IntPtr data);
        public static VertexAttribPointerDelegate VertexAttribPointer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void DrawElementsInstancedDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType,
            IntPtr offset, int instanceCount);
        public static DrawElementsInstancedDelegate DrawElementsInstanced;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        internal delegate void DrawElementsInstancedBaseInstanceDelegate(GLPrimitiveType primitiveType, int count, DrawElementsType elementType,
            IntPtr offset, int instanceCount, int baseInstance);
        internal static DrawElementsInstancedBaseInstanceDelegate DrawElementsInstancedBaseInstance;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void VertexAttribDivisorDelegate(int location, int frequency);
        public static VertexAttribDivisorDelegate VertexAttribDivisor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        public delegate void PatchParameteriDelegate(GLPatchParameter patchParameter, int value);
        public static PatchParameteriDelegate PatchParameteri;

#if DEBUG
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void DebugMessageCallbackProc(int source, int type, int id, int severity, int length, IntPtr message, IntPtr userParam);
        static DebugMessageCallbackProc DebugProc;
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        delegate void DebugMessageCallbackDelegate(DebugMessageCallbackProc callback, IntPtr userParam);
        static DebugMessageCallbackDelegate DebugMessageCallback;

        public delegate void ErrorDelegate(string message);
        public static event ErrorDelegate OnError;

        static void DebugMessageCallbackHandler(int source, int type, int id, int severity, int length, IntPtr message, IntPtr userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);
            System.Diagnostics.Debug.WriteLine(errorMessage);
            if (OnError != null)
                OnError(errorMessage);
        }
#endif

        private static T LoadFunction<T>(string function, bool throwIfNotFound = true)
        {
            var ret = Sdl.GL.GetProcAddress(function);

            if (ret == IntPtr.Zero)
            {
                if (throwIfNotFound)
                    throw new EntryPointNotFoundException(function);

                return default(T);
            }

            return Marshal.GetDelegateForFunctionPointer<T>(ret);
        }

        public static void LoadEntryPoints()
        {
            LoadPlatformEntryPoints();

            if (Viewport == null)
                Viewport = LoadFunction<ViewportDelegate>("glViewport");
            if (Scissor == null)
                Scissor = LoadFunction<ScissorDelegate>("glScissor");
            //if (MakeCurrent == null)
            //    MakeCurrent = LoadFunction<MakeCurrentDelegate> ("glMakeCurrent");

            GetError = LoadFunction<GetErrorDelegate>("glGetError");

            TexParameterf = LoadFunction<TexParameterFloatDelegate>("glTexParameterf");
            TexParameterfv = LoadFunction<TexParameterFloatArrayDelegate>("glTexParameterfv");
            TexParameteri = LoadFunction<TexParameterIntDelegate>("glTexParameteri");

            EnableVertexAttribArray = LoadFunction<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            DisableVertexAttribArray = LoadFunction<DisableVertexAttribArrayDelegate>("glDisableVertexAttribArray");
            GetIntegerv = LoadFunction<GetIntegerDelegate>("glGetIntegerv");
            GetStringInternal = LoadFunction<GetStringDelegate>("glGetString");
            ClearDepth = LoadFunction<ClearDepthDelegate>("glClearDepth");
            if (ClearDepth == null)
                ClearDepth = LoadFunction<ClearDepthDelegate>("glClearDepthf");
            DepthRanged = LoadFunction<DepthRangedDelegate>("glDepthRange");
            DepthRangef = LoadFunction<DepthRangefDelegate>("glDepthRangef");
            Clear = LoadFunction<ClearDelegate>("glClear");
            ClearColor = LoadFunction<ClearColorDelegate>("glClearColor");
            ClearStencil = LoadFunction<ClearStencilDelegate>("glClearStencil");
            Flush = LoadFunction<FlushDelegate>("glFlush");
            GenTextures = LoadFunction<GenTexturesDelegte>("glGenTextures");
            BindTexture = LoadFunction<BindTextureDelegate>("glBindTexture");

            Enable = LoadFunction<EnableDelegate>("glEnable");
            Disable = LoadFunction<DisableDelegate>("glDisable");
            CullFace = LoadFunction<CullFaceDelegate>("glCullFace");
            FrontFace = LoadFunction<FrontFaceDelegate>("glFrontFace");
            PolygonMode = LoadFunction<PolygonModeDelegate>("glPolygonMode");
            PolygonOffset = LoadFunction<PolygonOffsetDelegate>("glPolygonOffset");

            BindBuffer = LoadFunction<BindBufferDelegate>("glBindBuffer");
            DrawBuffers = LoadFunction<DrawBuffersDelegate>("glDrawBuffers");
            DrawElements = LoadFunction<DrawElementsDelegate>("glDrawElements");
            DrawArrays = LoadFunction<DrawArraysDelegate>("glDrawArrays");
            ReadPixelsInternal = LoadFunction<ReadPixelsDelegate>("glReadPixels");

            ReadBuffer = LoadFunction<ReadBufferDelegate>("glReadBuffer");
            DrawBuffer = LoadFunction<DrawBufferDelegate>("glDrawBuffer");

            Uniform1i = LoadFunction<Uniform1iDelegate>("glUniform1i");
            Uniform1iv = LoadFunction<Uniform1ivDelegate>("glUniform1iv");
            Uniform2iv = LoadFunction<Uniform2ivDelegate>("glUniform2iv");
            Uniform3iv = LoadFunction<Uniform3ivDelegate>("glUniform3iv");
            Uniform4iv = LoadFunction<Uniform4ivDelegate>("glUniform4iv");
            Uniform1fv = LoadFunction<Uniform1fvDelegate>("glUniform1fv");
            Uniform2fv = LoadFunction<Uniform2fvDelegate>("glUniform2fv");
            Uniform3fv = LoadFunction<Uniform3fvDelegate>("glUniform3fv");
            Uniform4fv = LoadFunction<Uniform4fvDelegate>("glUniform4fv");
            Uniform2fv = LoadFunction<Uniform2fvDelegate>("glUniform2fv");
            Uniform3fv = LoadFunction<Uniform3fvDelegate>("glUniform3fv");
            Uniform4fv = LoadFunction<Uniform4fvDelegate>("glUniform4fv");
            UniformMatrix2fv = LoadFunction<UniformMatrix2fvDelegate>("glUniformMatrix2fv");
            UniformMatrix3fv = LoadFunction<UniformMatrix3fvDelegate>("glUniformMatrix3fv");
            UniformMatrix4fv = LoadFunction<UniformMatrix4fvDelegate>("glUniformMatrix4fv");

            GenRenderbuffers = LoadFunction<GenRenderbuffersDelegate>("glGenRenderbuffers");
            BindRenderbuffer = LoadFunction<BindRenderbufferDelegate>("glBindRenderbuffer");
            DeleteRenderbuffers = LoadFunction<DeleteRenderbuffersDelegate>("glDeleteRenderbuffers");
            GenFramebuffers = LoadFunction<GenFramebuffersDelegate>("glGenFramebuffers");
            BindFramebuffer = LoadFunction<BindFramebufferDelegate>("glBindFramebuffer");
            DeleteFramebuffers = LoadFunction<DeleteFramebuffersDelegate>("glDeleteFramebuffers");
            FramebufferTexture2D = LoadFunction<FramebufferTexture2DDelegate>("glFramebufferTexture2D");
            FramebufferRenderbuffer = LoadFunction<FramebufferRenderbufferDelegate>("glFramebufferRenderbuffer");
            RenderbufferStorage = LoadFunction<RenderbufferStorageDelegate>("glRenderbufferStorage");
            RenderbufferStorageMultisample = LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisample");
            GenerateMipmap = LoadFunction<GenerateMipmapDelegate>("glGenerateMipmap");
            BlitFramebuffer = LoadFunction<BlitFramebufferDelegate>("glBlitFramebuffer");
            CheckFramebufferStatus = LoadFunction<CheckFramebufferStatusDelegate>("glCheckFramebufferStatus");

            GenQueries = LoadFunction<GenQueriesDelegate>("glGenQueries");
            BeginQuery = LoadFunction<BeginQueryDelegate>("glBeginQuery");
            EndQuery = LoadFunction<EndQueryDelegate>("glEndQuery");
            GetQueryObject = LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectuiv");
            if (GetQueryObject == null)
                GetQueryObject = LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectivARB");
            if (GetQueryObject == null)
                GetQueryObject = LoadFunction<GetQueryObjectDelegate>("glGetQueryObjectiv");
            DeleteQueries = LoadFunction<DeleteQueriesDelegate>("glDeleteQueries");

            ActiveTexture = LoadFunction<ActiveTextureDelegate>("glActiveTexture");
            CreateShader = LoadFunction<CreateShaderDelegate>("glCreateShader");
            ShaderSourceInternal = LoadFunction<ShaderSourceDelegate>("glShaderSource");
            CompileShader = LoadFunction<CompileShaderDelegate>("glCompileShader");
            GetShaderiv = LoadFunction<GetShaderDelegate>("glGetShaderiv");
            GetShaderInfoLogInternal = LoadFunction<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
            IsShader = LoadFunction<IsShaderDelegate>("glIsShader");
            DeleteShader = LoadFunction<DeleteShaderDelegate>("glDeleteShader");
            GetAttribLocation = LoadFunction<GetAttribLocationDelegate>("glGetAttribLocation");
            BindAttribLocation = LoadFunction<BindAttribLocationDelegate>("glBindAttribLocation");
            GetUniformLocation = LoadFunction<GetUniformLocationDelegate>("glGetUniformLocation");

            GetUniformBlockIndex = LoadFunction<GetUniformBlockIndexDelegate>("glGetUniformBlockIndex");
            GetUniformIndicesInternal = LoadFunction<GetUniformIndicesDelegate>("glGetUniformIndices");

            IsProgram = LoadFunction<IsProgramDelegate>("glIsProgram");
            DeleteProgram = LoadFunction<DeleteProgramDelegate>("glDeleteProgram");
            CreateProgram = LoadFunction<CreateProgramDelegate>("glCreateProgram");
            AttachShader = LoadFunction<AttachShaderDelegate>("glAttachShader");
            UseProgram = LoadFunction<UseProgramDelegate>("glUseProgram");
            LinkProgram = LoadFunction<LinkProgramDelegate>("glLinkProgram");
            GetProgramiv = LoadFunction<GetProgramDelegate>("glGetProgramiv");
            GetProgramInfoLogInternal = LoadFunction<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
            DetachShader = LoadFunction<DetachShaderDelegate>("glDetachShader");

            BlendColor = LoadFunction<BlendColorDelegate>("glBlendColor");
            BlendEquationSeparate = LoadFunction<BlendEquationSeparateDelegate>("glBlendEquationSeparate");
            BlendFuncSeparate = LoadFunction<BlendFuncSeparateDelegate>("glBlendFuncSeparate");
            ColorMask = LoadFunction<ColorMaskDelegate>("glColorMask");
            DepthFunc = LoadFunction<DepthFuncDelegate>("glDepthFunc");
            DepthMask = LoadFunction<DepthMaskDelegate>("glDepthMask");
            StencilFuncSeparate = LoadFunction<StencilFuncSeparateDelegate>("glStencilFuncSeparate");
            StencilOpSeparate = LoadFunction<StencilOpSeparateDelegate>("glStencilOpSeparate");
            StencilFunc = LoadFunction<StencilFuncDelegate>("glStencilFunc");
            StencilOp = LoadFunction<StencilOpDelegate>("glStencilOp");
            StencilMask = LoadFunction<StencilMaskDelegate>("glStencilMask");

            CompressedTexImage2D = LoadFunction<CompressedTexImage2DDelegate>("glCompressedTexImage2D");
            TexImage2D = LoadFunction<TexImage2DDelegate>("glTexImage2D");
            CompressedTexSubImage2D = LoadFunction<CompressedTexSubImage2DDelegate>("glCompressedTexSubImage2D");
            TexSubImage2D = LoadFunction<TexSubImage2DDelegate>("glTexSubImage2D");
            PixelStore = LoadFunction<PixelStoreDelegate>("glPixelStorei");
            Finish = LoadFunction<FinishDelegate>("glFinish");
            GetTexImageInternal = LoadFunction<GetTexImageDelegate>("glGetTexImage");
            GetCompressedTexImageInternal = LoadFunction<GetCompressedTexImageDelegate>("glGetCompressedTexImage");
            TexImage3D = LoadFunction<TexImage3DDelegate>("glTexImage3D");
            TexSubImage3D = LoadFunction<TexSubImage3DDelegate>("glTexSubImage3D");
            DeleteTextures = LoadFunction<DeleteTexturesDelegate>("glDeleteTextures");

            GenBuffers = LoadFunction<GenBuffersDelegate>("glGenBuffers");
            BufferData = LoadFunction<BufferDataDelegate>("glBufferData");
            MapBuffer = LoadFunction<MapBufferDelegate>("glMapBuffer");
            UnmapBuffer = LoadFunction<UnmapBufferDelegate>("glUnmapBuffer");
            BufferSubData = LoadFunction<BufferSubDataDelegate>("glBufferSubData");
            DeleteBuffers = LoadFunction<DeleteBuffersDelegate>("glDeleteBuffers");

            VertexAttribPointer = LoadFunction<VertexAttribPointerDelegate>("glVertexAttribPointer");

            PatchParameteri = LoadFunction<PatchParameteriDelegate>("glPatchParameteri");

            GetProgramInterfaceiv = LoadFunction<GetProgramInterfaceDelegate>("glGetProgramInterfaceiv");
            GetProgramResourceiv = LoadFunction<GetProgramResourceDelegate>("glGetProgramResourceiv");
            GetProgramResourceNameInternal = LoadFunction<GetProgramResourceNameDelegate>("glGetProgramResourceName");

            // Instanced drawing requires GL 3.2 or up, if the either of the following entry points can not be loaded
            // this will get flagged by setting SupportsInstancing in GraphicsCapabilities to false.
            try
            {
                DrawElementsInstanced = LoadFunction<DrawElementsInstancedDelegate>("glDrawElementsInstanced");
                VertexAttribDivisor = LoadFunction<VertexAttribDivisorDelegate>("glVertexAttribDivisor");
                DrawElementsInstancedBaseInstance = LoadFunction<DrawElementsInstancedBaseInstanceDelegate>("glDrawElementsInstancedBaseInstance");
            }
            catch (EntryPointNotFoundException) { }

#if DEBUG
            try
            {
                DebugMessageCallback = LoadFunction<DebugMessageCallbackDelegate>("glDebugMessageCallback");
                if (DebugMessageCallback != null)
                {
                    DebugProc = DebugMessageCallbackHandler;
                    DebugMessageCallback(DebugProc, IntPtr.Zero);
                    Enable(EnableCap.DebugOutput);
                    Enable(EnableCap.DebugOutputSynchronous);
                }
            }
            catch (EntryPointNotFoundException)
            {
                // Ignore the debug message callback if the entry point can not be found
            }
#endif
            if (BoundApi == RenderApi.ES)
            {
                InvalidateFramebuffer = LoadFunction<InvalidateFramebufferDelegate>("glDiscardFramebufferEXT");
            }

            LoadExtensions();
        }

        public static List<string> Extensions = new List<string>();

        public static void LoadExtensions()
        {
            string extstring = GL.GetString(StringName.Extensions);
            var error = GL.GetError();
            if (!string.IsNullOrEmpty(extstring) && error == ErrorCode.NoError)
                Extensions.AddRange(extstring.Split(' '));

            // now load Extensions :)
            if (GL.GenRenderbuffers == null && Extensions.Contains("GL_EXT_framebuffer_object"))
            {
                GL.LoadFrameBufferObjectEXTEntryPoints();
            }
            if (GL.RenderbufferStorageMultisample == null)
            {
                if (Extensions.Contains("GL_APPLE_framebuffer_multisample"))
                {
                    GL.RenderbufferStorageMultisample = LoadFunction<GL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleAPPLE");
                    GL.BlitFramebuffer = LoadFunction<GL.BlitFramebufferDelegate>("glResolveMultisampleFramebufferAPPLE");
                }
                else if (Extensions.Contains("GL_EXT_multisampled_render_to_texture"))
                {
                    GL.RenderbufferStorageMultisample = LoadFunction<GL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
                    GL.FramebufferTexture2DMultiSample = LoadFunction<GL.FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleEXT");

                }
                else if (Extensions.Contains("GL_IMG_multisampled_render_to_texture"))
                {
                    GL.RenderbufferStorageMultisample = LoadFunction<GL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleIMG");
                    GL.FramebufferTexture2DMultiSample = LoadFunction<GL.FramebufferTexture2DMultiSampleDelegate>("glFramebufferTexture2DMultisampleIMG");
                }
                else if (Extensions.Contains("GL_NV_framebuffer_multisample"))
                {
                    GL.RenderbufferStorageMultisample = LoadFunction<GL.RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleNV");
                    GL.BlitFramebuffer = LoadFunction<GL.BlitFramebufferDelegate>("glBlitFramebufferNV");
                }
            }
        }

        public static void LoadFrameBufferObjectARBEntryPoints()
        {
            GenRenderbuffers = LoadFunction<GenRenderbuffersDelegate>("glGenRenderbuffers");
            BindRenderbuffer = LoadFunction<BindRenderbufferDelegate>("glBindRenderbuffer");
            DeleteRenderbuffers = LoadFunction<DeleteRenderbuffersDelegate>("glDeleteRenderbuffers");
            GenFramebuffers = LoadFunction<GenFramebuffersDelegate>("glGenFramebuffers");
            BindFramebuffer = LoadFunction<BindFramebufferDelegate>("glBindFramebuffer");
            DeleteFramebuffers = LoadFunction<DeleteFramebuffersDelegate>("glDeleteFramebuffers");
            FramebufferTexture2D = LoadFunction<FramebufferTexture2DDelegate>("glFramebufferTexture2D");
            FramebufferRenderbuffer = LoadFunction<FramebufferRenderbufferDelegate>("glFramebufferRenderbuffer");
            RenderbufferStorageMultisample = LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisample");
            GenerateMipmap = LoadFunction<GenerateMipmapDelegate>("glGenerateMipmap");
            BlitFramebuffer = LoadFunction<BlitFramebufferDelegate>("glBlitFramebuffer");
            CheckFramebufferStatus = LoadFunction<CheckFramebufferStatusDelegate>("glCheckFramebufferStatus");
        }

        public static void LoadFrameBufferObjectEXTEntryPoints()
        {
            GenRenderbuffers = LoadFunction<GenRenderbuffersDelegate>("glGenRenderbuffersEXT");
            BindRenderbuffer = LoadFunction<BindRenderbufferDelegate>("glBindRenderbufferEXT");
            DeleteRenderbuffers = LoadFunction<DeleteRenderbuffersDelegate>("glDeleteRenderbuffersEXT");
            GenFramebuffers = LoadFunction<GenFramebuffersDelegate>("glGenFramebuffersEXT");
            BindFramebuffer = LoadFunction<BindFramebufferDelegate>("glBindFramebufferEXT");
            DeleteFramebuffers = LoadFunction<DeleteFramebuffersDelegate>("glDeleteFramebuffersEXT");
            FramebufferTexture2D = LoadFunction<FramebufferTexture2DDelegate>("glFramebufferTexture2DEXT");
            FramebufferRenderbuffer = LoadFunction<FramebufferRenderbufferDelegate>("glFramebufferRenderbufferEXT");
            RenderbufferStorage = LoadFunction<RenderbufferStorageDelegate>("glRenderbufferStorageEXT");
            RenderbufferStorageMultisample = LoadFunction<RenderbufferStorageMultisampleDelegate>("glRenderbufferStorageMultisampleEXT");
            GenerateMipmap = LoadFunction<GenerateMipmapDelegate>("glGenerateMipmapEXT");
            BlitFramebuffer = LoadFunction<BlitFramebufferDelegate>("glBlitFramebufferEXT");
            CheckFramebufferStatus = LoadFunction<CheckFramebufferStatusDelegate>("glCheckFramebufferStatusEXT");
        }

        public static void LoadPlatformEntryPoints()
        {
            BoundApi = RenderApi.GL;
        }

        public static GraphicsContext CreateContext(IntPtr windowHandle) => new GraphicsContext(windowHandle);

        /* Helper Functions */

        public static void DepthRange(float min, float max)
        {
            if (BoundApi == RenderApi.ES)
                DepthRangef(min, max);
            else
                DepthRanged(min, max);
        }

        public static void Uniform1(int location, int value)
        {
            Uniform1i(location, value);
        }

        public static unsafe void UniformMatrix2(int location, int size, byte* values)
        {
            UniformMatrix2fv(location, size, true, values);
        }

        public static unsafe void UniformMatrix3(int location, int size, byte* values)
        {
            UniformMatrix3fv(location, size, true, values);
        }

        public static unsafe void UniformMatrix4(int location, int size, byte* values)
        {
            UniformMatrix4fv(location, size, true, values);
        }

        public unsafe static string GetString(StringName name)
        {
            return Marshal.PtrToStringAnsi(GetStringInternal(name));
        }

        protected static IntPtr MarshalStringArrayToPtr(string[] strings)
        {
            IntPtr intPtr = IntPtr.Zero;
            if (strings != null && strings.Length != 0)
            {
                intPtr = Marshal.AllocHGlobal(strings.Length * IntPtr.Size);
                if (intPtr == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                int i = 0;
                try
                {
                    for (i = 0; i < strings.Length; i++)
                    {
                        IntPtr val = MarshalStringToPtr(strings[i]);
                        Marshal.WriteIntPtr(intPtr, i * IntPtr.Size, val);
                    }
                }
                catch (OutOfMemoryException)
                {
                    for (i--; i >= 0; i--)
                    {
                        Marshal.FreeHGlobal(Marshal.ReadIntPtr(intPtr, i * IntPtr.Size));
                    }
                    Marshal.FreeHGlobal(intPtr);
                    throw;
                }
            }
            return intPtr;
        }

        protected unsafe static IntPtr MarshalStringToPtr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return IntPtr.Zero;
            }
            int num = Encoding.ASCII.GetMaxByteCount(str.Length) + 1;
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            if (intPtr == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            fixed (char* chars = str + str.GetPinnableReference() / 2)
            {
                int bytes = Encoding.ASCII.GetBytes(chars, str.Length, (byte*)((void*)intPtr), num);
                Marshal.WriteByte(intPtr, bytes, 0);
                return intPtr;
            }
        }

        protected static void FreeStringArrayPtr(IntPtr ptr, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Marshal.FreeHGlobal(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
            }
            Marshal.FreeHGlobal(ptr);
        }

        public static string GetProgramInfoLog(int programId)
        {
            int length = 0;
            GetProgram(programId, GetProgramParameterName.LogLength, out length);
            var sb = new StringBuilder(length);
            GetProgramInfoLogInternal(programId, length, IntPtr.Zero, sb);

            return sb.ToString();
        }

        public static string GetShaderInfoLog(int shaderId)
        {
            int length = 0;
            GetShader(shaderId, ShaderParameter.LogLength, out length);
            var sb = new StringBuilder(length);
            GetShaderInfoLogInternal(shaderId, length, IntPtr.Zero, sb);

            return sb.ToString();
        }

        public unsafe static void ShaderSource(int shaderId, string code)
        {
            int length = code.Length;
            IntPtr intPtr = MarshalStringArrayToPtr(new string[] { code });
            ShaderSourceInternal(shaderId, 1, intPtr, &length);
            FreeStringArrayPtr(intPtr, 1);
        }

        public unsafe static void GetShader(int shaderId, ShaderParameter name, out int result)
        {
            fixed (int* ptr = &result)
            {
                GetShaderiv(shaderId, (int)name, ptr);
            }
        }

        public unsafe static void GetUniformIndices(int programId, int uniformCount, string[] uniformNames, out int[] result)
        {
            result = new int[uniformCount];
            fixed (int* ptr = &result[0])
            {
                GetUniformIndicesInternal(programId, uniformCount, uniformNames, ptr);
            }
        }

        public unsafe static void GetProgramInterface(int programId, ProgramInterface programInterface, ProgramInterfaceName name, out int programParams)
        {
            fixed (int* ptr = &programParams)
            {
                GetProgramInterfaceiv(programId, (int)programInterface, (int)name, ptr);
            }
        }

        public unsafe static void GetProgramResource(int programId, ProgramInterface programInterface, int index,
            ProgramResourceProperties[] props, out int length, out int[] result)
        {
            int[] x = props.Cast<int>().ToArray();
            result = new int[x.Length];

            fixed (int* pr = &x[0], l = &length, r = &result[0])
            {
                GetProgramResourceiv(programId, (int)programInterface, index, x.Length, pr, x.Length, l, r);
            }
        }

        public unsafe static void GetProgramResourceName(int programId, ProgramInterface programInterface, int index,
            int count, out int length, out string name)
        {
            StringBuilder sb = new StringBuilder();

            fixed (int* l = &length)
            {
                GetProgramResourceNameInternal(programId, (int)programInterface, index, count, l, sb);
            }

            name = sb.ToString();
        }

        public unsafe static void GetProgram(int programId, GetProgramParameterName name, out int result)
        {
            fixed (int* ptr = &result)
            {
                GetProgramiv(programId, (int)name, ptr);
            }
        }

        public unsafe static void GetInteger(GetPName name, out int value)
        {
            fixed (int* ptr = &value)
            {
                GetIntegerv((int)name, ptr);
            }
        }

        public unsafe static void GetInteger(int name, out int value)
        {
            fixed (int* ptr = &value)
            {
                GetIntegerv(name, ptr);
            }
        }

        public static void TexParameter(TextureTarget target, TextureParameterName name, float value)
        {
            TexParameterf(target, name, value);
        }

        public unsafe static void TexParameter(TextureTarget target, TextureParameterName name, float[] values)
        {
            fixed (float* ptr = &values[0])
            {
                TexParameterfv(target, name, ptr);
            }
        }

        public static void TexParameter(TextureTarget target, TextureParameterName name, int value)
        {
            TexParameteri(target, name, value);
        }

        public static void GetTexImage<T>(TextureTarget target, int level, PixelFormat format, PixelType type, T[] pixels) where T : struct
        {
            var pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTexImageInternal(target, level, format, type, pixelsPtr.AddrOfPinnedObject());
            }
            finally
            {
                pixelsPtr.Free();
            }
        }

        public static void GetCompressedTexImage<T>(TextureTarget target, int level, T[] pixels) where T : struct
        {
            var pixelsPtr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetCompressedTexImageInternal(target, level, pixelsPtr.AddrOfPinnedObject());
            }
            finally
            {
                pixelsPtr.Free();
            }
        }

        public static void ReadPixels<T>(int x, int y, int width, int height, PixelFormat format, PixelType type, T[] data)
        {
            var dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                ReadPixelsInternal(x, y, width, height, format, type, dataPtr.AddrOfPinnedObject());
            }
            finally
            {
                dataPtr.Free();
            }
        }
    }
}

