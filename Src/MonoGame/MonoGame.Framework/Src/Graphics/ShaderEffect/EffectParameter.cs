using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectParameter
	{
        private string name;
        private EffectParameterClass parameterClass;
        private EffectParameterType parameterType;
        private int arraySize;
        private int primitiveSize;
        private int rowCount;
        private int columnCount;
        private EffectParameterCollection elements;
        private object dat;

        private byte[] buffer;
        private unsafe delegate void UpdateParamDelegate(int location, int size, byte* values);
        private UpdateParamDelegate updateParam;

        public string Name => name;
        public EffectParameterClass ParameterClass => parameterClass;
        public EffectParameterType ParameterType => parameterType;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;
        public EffectParameterCollection Elements => elements;
        public int ArraySize => arraySize;
        public int PrimitiveSize => primitiveSize;

        public object Data
        {
            get { return dat; }
            set
            {
                dat = value;
                Dirty = true;
            }
        }

        private bool Dirty { get; set; }

        public EffectParameter() { }

        public override string ToString() => string.Concat("[", ParameterClass, " ", ParameterType, "] ", Name, " : ", GetDataValueString());
        
        public EffectParameter(EffectParameterClass c, EffectParameterType t, string name, 
            int rowCount, int columnCount, EffectParameterCollection elements, object data)
		{
            this.parameterClass = c;
            this.parameterType = t;
            this.name = name;
            this.rowCount = rowCount;
			this.columnCount = columnCount;
            this.elements = elements;

            this.arraySize = elements.Count == 0 ? 1 : elements.Count;
            
            if (data != null) 
            {
                Array a = data as Array;
                this.primitiveSize = Marshal.SizeOf(data.GetType().GetElementType());
                this.buffer = new byte[a.Length * this.primitiveSize];
            }
            else
            {
                if (elements.Count > 1)
                {
                    Debugger.Break();
                }
            }

            Data = data;

            updateParam = GetUpdateParam();

            Dirty = true;
		}

        private unsafe UpdateParamDelegate GetUpdateParam()
        {
            switch (parameterClass)
            {
                case EffectParameterClass.Scalar:
                    switch (parameterType)
                    {
                        case EffectParameterType.Bool:
                        case EffectParameterType.Int32:
                            return new UpdateParamDelegate(GL.Uniform1iv);
                        case EffectParameterType.Single:
                            return new UpdateParamDelegate(GL.Uniform1fv);
                    }
                    break;
                case EffectParameterClass.Vector:
                    switch (columnCount)
                    {
                        case 2:
                            switch (parameterType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform2iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform2fv);
                            }
                            break;
                        case 3:
                            switch (parameterType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform3iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform3fv);
                            }
                            break;
                        case 4:
                            switch (parameterType)
                            {
                                case EffectParameterType.Bool:
                                case EffectParameterType.Int32:
                                    return new UpdateParamDelegate(GL.Uniform4iv);
                                case EffectParameterType.Single:
                                    return new UpdateParamDelegate(GL.Uniform4fv);
                            }
                            break;
                    }
                    break;
                case EffectParameterClass.Matrix:
                    switch (columnCount)
                    {
                        case 2:
                            return GL.UniformMatrix2;
                        case 3:
                            return GL.UniformMatrix3;
                        case 4:
                            return GL.UniformMatrix4;
                    }
                    break;
                case EffectParameterClass.Object:
                case EffectParameterClass.Struct:
                    return null;
            }
            throw new Exception("Did not find right delegate for parameter.");
        }

        private int SetParameter(int offset, EffectParameter param)
        {               
            var rowsUsed = 0;

            var el = param.Elements;
            if (el.Count > 0)
            {
                for (var i=0; i < el.Count; i++)
                {
                    var e = el[i];
                    int rowSize = e.PrimitiveSize * e.ColumnCount;
                    var rowsUsedSubParam = SetParameter(offset, el[i]);
                    offset += rowsUsedSubParam * rowSize;
                    rowsUsed += rowsUsedSubParam;
                }
            }
            else if (param.Data != null)
            {
                switch (param.ParameterType)
                {
                    case EffectParameterType.Single:
                    case EffectParameterType.Int32:
                    case EffectParameterType.Bool:
                        // HLSL assumes matrices are column-major, whereas in-memory we use row-major.
                        // TODO: HLSL can be told to use row-major. We should handle that too.
                        if (param.ParameterClass == EffectParameterClass.Matrix)
                        {
                            rowsUsed = param.ColumnCount;
                            SetData(offset, param.ColumnCount, param.RowCount, param.Data);
                        }
                        else
                        {
                            rowsUsed = param.RowCount;
                            SetData(offset, param.RowCount, param.ColumnCount, param.Data);
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported!");
                }
            }

            return rowsUsed;
        }

        private void SetData(int offset, int rows, int columns, object data)
        {
            // Shader registers are always 4 bytes and all the
            // incoming data objects should be 4 bytes per element.
            const int elementSize = 4;
            const int rowSize = elementSize * 4;

            // Take care of a single element.
            if (rows == 1 && columns == 1)
            {
                // EffectParameter stores all values in arrays by default.             
                Buffer.BlockCopy(data as Array, 0, buffer, offset, elementSize);
            }

            // Take care of the single copy case!
            else if (rows == 1 || (rows == 4 && columns == 4))
            {
                // take care of shader compiler optimization
                int len = rows * columns * elementSize;
                if (buffer.Length - offset > len)
                    len = buffer.Length - offset;
                Buffer.BlockCopy(data as Array, 0, buffer, offset, rows*columns*elementSize);
            }
            else
            {
                var source = data as Array;

                var stride = (columns*elementSize);
                for (var y = 0; y < rows; y++)
                    Buffer.BlockCopy(source, stride*y, buffer, offset + (rowSize*y), columns*elementSize);
            }
        }

        public void Apply()
        {
            if (!Dirty)
                return;
                
            if (updateParam == null)
                return;

            SetParameter(0, this);

            Dirty = false;
        }

        public void Update(int location)
        {
            if (updateParam == null)
                return;

            unsafe
            {
                fixed (byte* ptr = buffer)
                    updateParam(location, arraySize, ptr);

                GraphicsExtensions.CheckGLError();
            }
        }

        private string GetDataValueString()
        {
            string valueStr;

            if (Data == null)
            {
                if (elements == null)
                    valueStr = "(null)";
                else                
                    valueStr = string.Join(", ", elements.Select(e => e.GetDataValueString()));                
            }
            else
            {
                switch (parameterClass)
                {
                        // Object types are stored directly in the Data property.
                        // Display Data's string value.
                    case EffectParameterClass.Object:
                        valueStr = Data.ToString();
                        break;

                        // Matrix types are stored in a float[16] which we don't really have room for.
                        // Display "...".
                    case EffectParameterClass.Matrix:
                        valueStr = "...";
                        break;

                        // Scalar types are stored as a float[1].
                        // Display the first (and only) element's string value.                    
                    case EffectParameterClass.Scalar:
                        valueStr = (Data as Array).GetValue(0).ToString();
                        break;

                        // Vector types are stored as an Array<Type>.
                        // Display the string value of each array element.
                    case EffectParameterClass.Vector:
                        var array = Data as Array;
                        var arrayStr = new string[array.Length];
                        var idx = 0;
                        foreach (var e in array)
                        {
                            arrayStr[idx] = array.GetValue(idx).ToString();
                            idx++;
                        }

                        valueStr = string.Join(" ", arrayStr);
                        break;

                        // Handle additional cases here...
                    default:
                        valueStr = Data.ToString();
                        break;
                }
            }

            return string.Concat("{", valueStr, "}");                
        }

        public bool GetValueBoolean()
        {
            if (parameterClass != EffectParameterClass.Scalar || parameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

            return ((int[])Data)[0] != 0;
        }

        public int GetValueInt32()
        {
            if (parameterClass != EffectParameterClass.Scalar || parameterType != EffectParameterType.Int32)
                throw new InvalidCastException();

            return ((int[])Data)[0];
        }

		public Matrix4x4 GetValueMatrix ()
		{
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (rowCount != 4 || columnCount != 4)
                throw new InvalidCastException();

            var floatData = (float[])Data;

            return new Matrix4x4(  floatData[0], floatData[4], floatData[8], floatData[12],
                                floatData[1], floatData[5], floatData[9], floatData[13],
                                floatData[2], floatData[6], floatData[10], floatData[14],
                                floatData[3], floatData[7], floatData[11], floatData[15]);
		}
        
		public Matrix4x4[] GetValueMatrixArray (int count)
		{
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var ret = new Matrix4x4[count];
            for (var i = 0; i < count; i++)
                ret[i] = elements[i].GetValueMatrix();

		    return ret;
		}

		public Quaternion GetValueQuaternion ()
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            return new Quaternion(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
        }

		public Single GetValueSingle ()
		{
            // TODO: Should this fetch int and bool as a float?
            if (parameterClass != EffectParameterClass.Scalar || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

			return ((float[])Data)[0];
		}

		public Single[] GetValueSingleArray ()
		{
			if (elements != null && elements.Count > 0)
            {
                var ret = new Single[rowCount * columnCount * elements.Count];
				for (int i=0; i<elements.Count; i++)
                {
                    var elmArray = elements[i].GetValueSingleArray();
                    for (var j = 0; j < elmArray.Length; j++)
						ret[rowCount*columnCount*i+j] = elmArray[j];
				}
				return ret;
			}
			
			switch(parameterClass) 
            {
			case EffectParameterClass.Scalar:
				return new Single[] { GetValueSingle () };
            case EffectParameterClass.Vector:
			case EffectParameterClass.Matrix:
                    if (Data is Matrix4x4)
                    {
                        var matrix = (Matrix4x4)Data;

                        return new float[] {
							matrix.M11, matrix.M12, matrix.M13, matrix.M14,
							matrix.M21, matrix.M22, matrix.M23, matrix.M24,
							matrix.M31, matrix.M32, matrix.M33, matrix.M34,
							matrix.M41, matrix.M42, matrix.M43, matrix.M44
						};
                    }
                    else
                        return (float[])Data;
			default:
				throw new NotImplementedException();
			}
		}

		public string GetValueString ()
		{
            if (parameterClass != EffectParameterClass.Object || parameterType != EffectParameterType.String)
                throw new InvalidCastException();

		    return ((string[])Data)[0];
		}

		public Texture2D GetValueTexture2D ()
		{
            if (parameterClass != EffectParameterClass.Object || parameterType != EffectParameterType.Texture2D)
                throw new InvalidCastException();

			return (Texture2D)Data;
		}

		public TextureCube GetValueTextureCube ()
		{
            if (parameterClass != EffectParameterClass.Object || parameterType != EffectParameterType.TextureCube)
                throw new InvalidCastException();

            return (TextureCube)Data;
		}

		public Vector2 GetValueVector2 ()
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
			return new Vector2(vecInfo[0],vecInfo[1]);
		}

		public Vector2[] GetValueVector2Array()
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();
			if (elements != null && elements.Count > 0)
			{
				Vector2[] result = new Vector2[elements.Count];
				for (int i = 0; i < elements.Count; i++)
				{
					var v = elements[i].GetValueSingleArray();
					result[i] = new Vector2(v[0], v[1]);
				}
			return result;
			}
			
		return null;
		}

		public Vector3 GetValueVector3 ()
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
			return new Vector3(vecInfo[0],vecInfo[1],vecInfo[2]);
		}

       public Vector3[] GetValueVector3Array()
        {
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (elements != null && elements.Count > 0)
            {
                Vector3[] result = new Vector3[elements.Count];
                for (int i = 0; i < elements.Count; i++)
                {
                    var v = elements[i].GetValueSingleArray();
                    result[i] = new Vector3(v[0], v[1], v[2]);
                }
                return result;
            }
            return null;
        }

		public Vector4 GetValueVector4 ()
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
			return new Vector4(vecInfo[0],vecInfo[1],vecInfo[2],vecInfo[3]);
		}
        
        public Vector4[] GetValueVector4Array()
        {
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (elements != null && elements.Count > 0)
            {
                Vector4[] result = new Vector4[elements.Count];
                for (int i = 0; i < elements.Count; i++)
                {
                    var v = elements[i].GetValueSingleArray();
                    result[i] = new Vector4(v[0], v[1],v[2], v[3]);
                }
                return result;
            }
            return null;
        }

		public void SetValue (bool value)
		{
            if (parameterClass != EffectParameterClass.Scalar || parameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

            ((int[])Data)[0] = value ? 1 : 0;
            Dirty = true;
		}

		public void SetValue (int value)
		{
            if (parameterClass != EffectParameterClass.Scalar || parameterType != EffectParameterType.Int32)
                throw new InvalidCastException();

            ((int[])Data)[0] = value;
            Dirty = true;
		}

        public void SetValue(Matrix4x4 value)
        {
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            // HLSL expects matrices to be transposed by default.
            // These unrolled loops do the transpose during assignment.
            if (rowCount == 4 && columnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;

                fData[12] = value.M14;
                fData[13] = value.M24;
                fData[14] = value.M34;
                fData[15] = value.M44;
            }
            else if (rowCount == 4 && columnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;
            }
            else if (rowCount == 3 && columnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;

                fData[9] = value.M14;
                fData[10] = value.M24;
                fData[11] = value.M34;
            }
            else if (rowCount == 3 && columnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;
            }
            else if (rowCount == 3 && columnCount == 2)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;
            }

            Dirty = true;
        }

		public void SetValueTranspose(Matrix4x4 value)
		{
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            // HLSL expects matrices to be transposed by default, so copying them straight
            // from the in-memory version effectively transposes them back to row-major.
            if (rowCount == 4 && columnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;

                fData[12] = value.M41;
                fData[13] = value.M42;
                fData[14] = value.M43;
                fData[15] = value.M44;
            }
            else if (rowCount == 4 && columnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;

                fData[9] = value.M41;
                fData[10] = value.M42;
                fData[11] = value.M43;
            }
            else if (rowCount == 3 && columnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;
            }
            else if (rowCount == 3 && columnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;
            }
            else if (rowCount == 3 && columnCount == 2)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;
            }

			Dirty = true;
		}

		public void SetValue (Matrix4x4[] value)
		{
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

		    if (rowCount == 4 && columnCount == 4)
		    {
		        for (var i = 0; i < value.Length; i++)
		        {
		            var fData = (float[])elements[i].Data;

		            fData[0] = value[i].M11;
		            fData[1] = value[i].M21;
		            fData[2] = value[i].M31;
		            fData[3] = value[i].M41;

		            fData[4] = value[i].M12;
		            fData[5] = value[i].M22;
		            fData[6] = value[i].M32;
		            fData[7] = value[i].M42;

		            fData[8] = value[i].M13;
		            fData[9] = value[i].M23;
		            fData[10] = value[i].M33;
		            fData[11] = value[i].M43;

		            fData[12] = value[i].M14;
		            fData[13] = value[i].M24;
		            fData[14] = value[i].M34;
		            fData[15] = value[i].M44;
		        }
		    }
		    else if (rowCount == 4 && columnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;
                    fData[3] = value[i].M41;

                    fData[4] = value[i].M12;
                    fData[5] = value[i].M22;
                    fData[6] = value[i].M32;
                    fData[7] = value[i].M42;

                    fData[8] = value[i].M13;
                    fData[9] = value[i].M23;
                    fData[10] = value[i].M33;
                    fData[11] = value[i].M43;
                }
            }
            else if (rowCount == 3 && columnCount == 4)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;

                    fData[6] = value[i].M13;
                    fData[7] = value[i].M23;
                    fData[8] = value[i].M33;

                    fData[9] = value[i].M14;
                    fData[10] = value[i].M24;
                    fData[11] = value[i].M34;
                }
            }
            else if (rowCount == 3 && columnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;

                    fData[6] = value[i].M13;
                    fData[7] = value[i].M23;
                    fData[8] = value[i].M33;
                }
            }
            else if (rowCount == 3 && columnCount == 2)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;
                }
            }

            Dirty = true;
		}

        public void SetValueTranspose (Matrix4x4[] value)
        {
            if (parameterClass != EffectParameterClass.Matrix || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (rowCount == 4 && columnCount == 4)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M12;
                    fData[2] = value[i].M13;
                    fData[3] = value[i].M14;

                    fData[4] = value[i].M21;
                    fData[5] = value[i].M22;
                    fData[6] = value[i].M23;
                    fData[7] = value[i].M24;

                    fData[8] = value[i].M31;
                    fData[9] = value[i].M32;
                    fData[10] = value[i].M33;
                    fData[11] = value[i].M34;

                    fData[12] = value[i].M41;
                    fData[13] = value[i].M42;
                    fData[14] = value[i].M43;
                    fData[15] = value[i].M44;
                }
            }
            else if (rowCount == 4 && columnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;
                    fData[3] = value[i].M41;

                    fData[4] = value[i].M12;
                    fData[5] = value[i].M22;
                    fData[6] = value[i].M32;
                    fData[7] = value[i].M42;

                    fData[8] = value[i].M13;
                    fData[9] = value[i].M23;
                    fData[10] = value[i].M33;
                    fData[11] = value[i].M43;
                }
            }
            else if (rowCount == 3 && columnCount == 4)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M12;
                    fData[2] = value[i].M13;

                    fData[3] = value[i].M21;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M23;

                    fData[6] = value[i].M31;
                    fData[7] = value[i].M32;
                    fData[8] = value[i].M33;

                    fData[9] = value[i].M41;
                    fData[10] = value[i].M42;
                    fData[11] = value[i].M43;
                }
            }
            else if (rowCount == 3 && columnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M12;
                    fData[2] = value[i].M13;

                    fData[3] = value[i].M21;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M23;

                    fData[6] = value[i].M31;
                    fData[7] = value[i].M32;
                    fData[8] = value[i].M33;
                }
            }
            else if (rowCount == 3 && columnCount == 2)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M12;
                    fData[2] = value[i].M13;

                    fData[3] = value[i].M21;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M23;
                }
            }

            Dirty = true;
        }

		public void SetValue (Quaternion value)
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;

            Dirty = true;
		}

		public void SetValue (Single value)
		{
            if (parameterType != EffectParameterType.Single)
                throw new InvalidCastException();
			((float[])Data)[0] = value;

            Dirty = true;
		}

		public void SetValue (Single[] value)
		{
			for (var i = 0; i < value.Length; i++)
				elements[i].SetValue (value[i]);

            Dirty = true;
		}
		
		public void SetValue (Texture value)
		{
            if (this.parameterType != EffectParameterType.Texture && 
                this.parameterType != EffectParameterType.Texture1D &&
                this.parameterType != EffectParameterType.Texture2D &&
                this.parameterType != EffectParameterType.Texture3D &&
                this.parameterType != EffectParameterType.TextureCube) 
            {
                throw new InvalidCastException();
            }

			Data = value;

            Dirty = true;
		}

		public void SetValue (Vector2 value)
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            
            Dirty = true;
		}

		public void SetValue (Vector2[] value)
		{
            for (var i = 0; i < value.Length; i++)
				elements[i].SetValue (value[i]);
            
            Dirty = true;
		}

		public void SetValue (Vector3 value)
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            
            Dirty = true;
		}

		public void SetValue (Vector3[] value)
		{
            for (var i = 0; i < value.Length; i++)
				elements[i].SetValue (value[i]);
            
            Dirty = true;
		}

		public void SetValue (Vector4 value)
		{
            if (parameterClass != EffectParameterClass.Vector || parameterType != EffectParameterType.Single)
                throw new InvalidCastException();

			var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;
            
            Dirty = true;
		}

		public void SetValue (Vector4[] value)
		{
            for (var i = 0; i < value.Length; i++)
				elements[i].SetValue (value[i]);
            
            Dirty = true;
		}
	}    
}
