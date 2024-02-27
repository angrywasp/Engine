using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using AngryWasp.Helpers;

namespace Engine.Serializers
{
    public class MatrixSerializer : JsonConverter<Matrix4x4>
    {
        public string Serialize(Matrix4x4 value) => $"{{M11:{value.M11} M12:{value.M12} M13:{value.M13} M14:{value.M14} M21:{value.M21} M22:{value.M22} M23:{value.M23} M24:{value.M24} M31:{value.M31} M32:{value.M32} M33:{value.M33} M34:{value.M34} M41:{value.M41} M42:{value.M42} M43:{value.M43} M44:{value.M44}}}";

        public Matrix4x4 Deserialize(string value)
        {
            Dictionary<string, string>[] rowParameters = new Dictionary<string, string>[4];

            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 16)
                throw new FormatException("paramater is invalid format");

            float m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;

            if (!float.TryParse(namedParameters["M11"], out m11))
                throw new FormatException("paramater M11 is invalid format");

            if (!float.TryParse(namedParameters["M12"], out m12))
                throw new FormatException("paramater M12 is invalid format");

            if (!float.TryParse(namedParameters["M13"], out m13))
                throw new FormatException("paramater M13 is invalid format");

            if (!float.TryParse(namedParameters["M14"], out m14))
                throw new FormatException("paramater M14 is invalid format");

            if (!float.TryParse(namedParameters["M21"], out m21))
                throw new FormatException("paramater M21 is invalid format");

            if (!float.TryParse(namedParameters["M22"], out m22))
                throw new FormatException("paramater M22 is invalid format");

            if (!float.TryParse(namedParameters["M23"], out m23))
                throw new FormatException("paramater M23 is invalid format");

            if (!float.TryParse(namedParameters["M24"], out m24))
                throw new FormatException("paramater M24 is invalid format");

            if (!float.TryParse(namedParameters["M31"], out m31))
                throw new FormatException("paramater M31 is invalid format");

            if (!float.TryParse(namedParameters["M32"], out m32))
                throw new FormatException("paramater M32 is invalid format");

            if (!float.TryParse(namedParameters["M33"], out m33))
                throw new FormatException("paramater M33 is invalid format");

            if (!float.TryParse(namedParameters["M34"], out m34))
                throw new FormatException("paramater M34 is invalid format");

            if (!float.TryParse(namedParameters["M41"], out m41))
                throw new FormatException("paramater M41 is invalid format");

            if (!float.TryParse(namedParameters["M42"], out m42))
                throw new FormatException("paramater M42 is invalid format");

            if (!float.TryParse(namedParameters["M43"], out m43))
                throw new FormatException("paramater M43 is invalid format");

            if (!float.TryParse(namedParameters["M44"], out m44))
                throw new FormatException("paramater M44 is invalid format");

            return new Matrix4x4(
                m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44
            );
        }
    
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
