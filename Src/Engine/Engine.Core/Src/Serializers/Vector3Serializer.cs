using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using AngryWasp.Helpers;

namespace Engine.Serializers
{
    public class Vector3Serializer : JsonConverter<Vector3>
    {
        public string Serialize(Vector3 value) => $"{{X:{value.X} Y:{value.Y} Z:{value.Z}}}";

        public Vector3 Deserialize(string value)
        {
            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 3)
                throw new FormatException("paramater is invalid format");

            float x, y, z;

            if (!float.TryParse(namedParameters["X"], out x))
                throw new FormatException("paramater X is invalid format");

            if (!float.TryParse(namedParameters["Y"], out y))
                throw new FormatException("paramater Y is invalid format");

            if (!float.TryParse(namedParameters["Z"], out z))
                throw new FormatException("paramater Z is invalid format");

            return new Vector3(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
