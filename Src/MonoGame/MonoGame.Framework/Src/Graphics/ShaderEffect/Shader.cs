using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class VertexShaderBase : Shader
    {
        private VertexAttributeCollection attributes;

        public VertexAttributeCollection Attributes => attributes;

        public abstract override ShaderType ShaderType { get; }

        public int GetAttribLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Count; ++i)
            {
                if ((Attributes[i].Usage == usage) && (Attributes[i].Index == index))
                    return Attributes[i].Location;
            }
            return -1;
        }

        public void ParseAttributes(int program, string fileName)
        {
            int activeAttributeCount = -1;

            GL.GetProgramInterface(program, ProgramInterface.ProgramInput, ProgramInterfaceName.ActiveResources, out activeAttributeCount);
            GraphicsExtensions.CheckGLError();

            ProgramResourceProperties[] props = 
            {
                ProgramResourceProperties.NameLength,
                ProgramResourceProperties.Type,
                ProgramResourceProperties.Location
            };

            List<VertexAttribute> vertexAttributes = new List<VertexAttribute>();

            if (activeAttributeCount > Metadata.Layout.Count)
                throw new Exception($"{fileName}: Not all vertex attributes are defined in metadata");

            for (int i = 0; i < activeAttributeCount; i++)
            {
                var e = Metadata.Layout[i];
                vertexAttributes.Add(new VertexAttribute(e.Usage, e.UsageIndex, e.Location));
            }

            attributes = new VertexAttributeCollection(vertexAttributes);
        }
    }

    public class VertexShader : VertexShaderBase
    {
        public override ShaderType ShaderType => ShaderType.VertexShader;
    }

    public class TessellationControlShader : VertexShaderBase
    {
        public override ShaderType ShaderType => ShaderType.TessellationControlShader;
    }

    public class TessellationEvaluationShader : VertexShaderBase
    {
        public override ShaderType ShaderType => ShaderType.TessellationEvaluationShader;
    }

    public class PixelShader : Shader
    {
        private SamplerInfoCollection samplers;

        public SamplerInfoCollection Samplers => samplers;

        public override ShaderType ShaderType => ShaderType.FragmentShader;

        public void ParseUniforms(int program,
            ref Dictionary<string, EffectParameter> parameters,
            ref Dictionary<string, int> locations)
        {
            parameters = new Dictionary<string, EffectParameter>();
            locations = new Dictionary<string, int>();

            int activeUniformCount = -1;
            int activeUniformBlockCount = -1;

            GL.GetProgramInterface(program, ProgramInterface.Uniform, ProgramInterfaceName.ActiveResources, out activeUniformCount);
            GraphicsExtensions.CheckGLError();

            GL.GetProgramInterface(program, ProgramInterface.UniformBlock, ProgramInterfaceName.ActiveResources, out activeUniformBlockCount);
            GraphicsExtensions.CheckGLError();

            ProgramResourceProperties[] props = 
            {
                ProgramResourceProperties.NameLength,
                ProgramResourceProperties.Type,
                ProgramResourceProperties.Location,
                ProgramResourceProperties.ArraySize,
                ProgramResourceProperties.ArrayStride,
                ProgramResourceProperties.MatrixStride
            };

            if (activeUniformBlockCount > 0)
                throw new InvalidOperationException("GLSL shader parsing does not support uniform blocks");

            List<SamplerInfo> s = new List<SamplerInfo>();

            for (int i = 0; i < activeUniformCount; i++)
            {
                int length;
                int[] result;
                string name;

                GL.GetProgramResource(program, ProgramInterface.Uniform, i, props, out length, out result);
                GraphicsExtensions.CheckGLError();

                GL.GetProgramResourceName(program, ProgramInterface.Uniform, i, result[0], out length, out name);
                GraphicsExtensions.CheckGLError();

                int loc = result[2];
                int arraySize = result[3];
                ProgramResourceType prt = (ProgramResourceType)result[1];

                if (arraySize > 1)
                    name = name.Substring(0, name.Length - 3);

                if (locations.ContainsKey(name))
                    throw new ArgumentException($"Duplicate location key: {name}");

                if (parameters.ContainsKey(name))
                    throw new ArgumentException($"Duplicate parameter key: {name}");

                EffectParameter parameter = null;

                switch (prt)
                {
                    case ProgramResourceType.Sampler2d:
                    case ProgramResourceType.Sampler2dShadow:
                        {
                            s.Add(new SamplerInfo(name, SamplerType.Sampler2D, 
                                s.Count, parameters.Count, loc));

                            parameter = new EffectParameter
                            (
                                EffectParameterClass.Object,
                                EffectParameterType.Texture2D,
                                name, 0, 0,
                                EffectParameterCollection.Empty,
                                null
                            );
                        }
                        break;
                    case ProgramResourceType.SamplerCube:
                        {
                            s.Add(new SamplerInfo(name, SamplerType.SamplerCube, 
                                s.Count, parameters.Count, loc));

                            parameter = new EffectParameter
                            (
                                EffectParameterClass.Object,
                                EffectParameterType.TextureCube,
                                name, 0, 0,
                                EffectParameterCollection.Empty,
                                null
                            );
                        }
                        break;
                    case ProgramResourceType.Float:
                        parameter = Single(EffectParameterClass.Scalar, name, 1, 1, arraySize);
                        break;
                    case ProgramResourceType.Vec2:
                        parameter = Single(EffectParameterClass.Vector, name, 1, 2, arraySize);
                        break;
                    case ProgramResourceType.Vec3:
                        parameter = Single(EffectParameterClass.Vector, name, 1, 3, arraySize);
                        break;
                    case ProgramResourceType.Vec4:
                        parameter = Single(EffectParameterClass.Vector, name, 1, 4, arraySize);
                        break;
                    case ProgramResourceType.Mat2:
                        parameter = Single(EffectParameterClass.Matrix, name, 2, 2, arraySize);
                        break;
                    case ProgramResourceType.Mat3:
                        parameter = Single(EffectParameterClass.Matrix, name, 3, 3, arraySize);
                        break;
                    case ProgramResourceType.Mat4:
                        parameter = Single(EffectParameterClass.Matrix, name, 4, 4, arraySize);
                        break;
                    case ProgramResourceType.Bool:
                        parameter = Bool(EffectParameterClass.Scalar, name, 1, 1, arraySize);
                        break;
                    default:
                        throw new NotImplementedException($"ProgramResourceType {prt} is not implemented");
                }

                if (parameter == null)
                    throw new ArgumentNullException(nameof(parameter));

                parameters.Add(name, parameter);
                locations.Add(name, loc);
            }

            samplers = new SamplerInfoCollection(s.ToArray());

            if (Metadata.SamplerStates != null)
                foreach (var sm in Metadata.SamplerStates)
                {
                    if (samplers[sm.Name] == null)
                        continue;

                    samplers[sm.Name].State = new SamplerState
                    {
                        Name = sm.Name,
                        AddressU = sm.AddressU,
                        AddressV = sm.AddressV,
                        AddressW = sm.AddressW,
                        Filter = sm.Filter
                    };
                }
        }

        private EffectParameter Bool(EffectParameterClass epc, string name, int rowCount, int columnCount, int arraySize)
        {
            EffectParameterCollection elements = EffectParameterCollection.Empty;
            if (arraySize > 1)
            {
                EffectParameter[] p = new EffectParameter[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    p[i] = new EffectParameter
                    (
                        epc,
                        EffectParameterType.Bool,
                        $"{name}_{i}",
                        rowCount, //row count
                        columnCount, //column count
                        EffectParameterCollection.Empty,
                        new int[rowCount * columnCount]
                    );
                } 
            }

            return new EffectParameter
            (
                epc,
                EffectParameterType.Bool,
                name,
                rowCount, //row count
                columnCount, //column count
                elements,
                new int[rowCount * columnCount * arraySize]
            );
        }

        private EffectParameter Single(EffectParameterClass epc, string name, int rowCount, int columnCount, int arraySize)
        {
            EffectParameterCollection elements = EffectParameterCollection.Empty;
            if (arraySize > 1)
            {
                EffectParameter[] p = new EffectParameter[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    p[i] = new EffectParameter
                    (
                        epc,
                        EffectParameterType.Single,
                        $"{name}_{i}",
                        rowCount, //row count
                        columnCount, //column count
                        EffectParameterCollection.Empty,
                        new float[rowCount * columnCount]
                    );
                }
                elements = new EffectParameterCollection(p);
            }

            return new EffectParameter
            (
                epc,
                EffectParameterType.Single,
                name,
                rowCount, //row count
                columnCount, //column count
                elements,
                new float[rowCount * columnCount * arraySize]
            );
        }
    }

    public abstract class Shader : GraphicsResource
	{
        private int hashKey;
        private int handle = -1;
        private ShaderMetadata metadata;
        
        public int HashKey => hashKey;
        public abstract ShaderType ShaderType { get; }
        public int Handle => handle;

        public ShaderMetadata Metadata => metadata;

        public void SetMetadata(ShaderMetadata metadata)
        {
            this.metadata = metadata;
            Name = Path.GetFileName(metadata.ProgramPath);
            Tag = metadata.ProgramPath;
        }

        public bool Compile(GraphicsDevice device, string shaderText, out string log)
        {
            int shaderHandle = GL.CreateShader(ShaderType);
            GraphicsExtensions.CheckGLError();
            GL.ShaderSource(shaderHandle, shaderText);
            GraphicsExtensions.CheckGLError();
            GL.CompileShader(shaderHandle);
            GraphicsExtensions.CheckGLError();

            int compiled = 0;
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out compiled);
            GraphicsExtensions.CheckGLError();

            log = GL.GetShaderInfoLog(shaderHandle);

            if (compiled != (int)Bool.True)
            {
                if (device != null)
                    device.DisposeShader(shaderHandle);
                shaderHandle = -1;
                return false;
            }

            this.handle = shaderHandle;
            hashKey = MonoGame.Utilities.Hash.ComputeHash(Encoding.ASCII.GetBytes(shaderText));
            if (device != null)
                GraphicsDevice = device;
            return true;
        }

        internal protected override void GraphicsDeviceResetting()
        {
            if (handle != -1)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeShader(handle);
                handle = -1;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && handle != -1)
            {
                if (GraphicsDevice != null)
                    GraphicsDevice.DisposeShader(handle);
                handle = -1;
            }

            base.Dispose(disposing);
        }
	}
}

