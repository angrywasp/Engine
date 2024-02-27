using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using AngryWasp.Helpers;

namespace Engine.Serializers
{
    public class QuaternionSerializer : JsonConverter<Quaternion>
    {
        public string Serialize(Quaternion value) => $"{{X:{value.X} Y:{value.Y} Z:{value.Z} W:{value.W}}}";
        
        public Quaternion Deserialize(string value)
        {
            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 4)
                throw new FormatException("paramater is invalid format");

            float x, y, z, w;

            if (!float.TryParse(namedParameters["X"], out x))
                throw new FormatException("paramater X is invalid format");

            if (!float.TryParse(namedParameters["Y"], out y))
                throw new FormatException("paramater Y is invalid format");

            if (!float.TryParse(namedParameters["Z"], out z))
                throw new FormatException("paramater Z is invalid format");

            if (!float.TryParse(namedParameters["W"], out w))
                throw new FormatException("paramater Z is invalid format");

            return new Quaternion(x, y, z, w);
        }

        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
