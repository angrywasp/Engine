using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using AngryWasp.Helpers;

namespace Engine.Serializers
{
    public class Vector2Serializer : JsonConverter<Vector2>
    {
        public string Serialize(Vector2 value) => $"{{X:{value.X} Y:{value.Y}}}";

        public Vector2 Deserialize(string value)
        {
            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 2)
                throw new FormatException("paramater is invalid format");

            float x, y;

            if (!float.TryParse(namedParameters["X"], out x))
                throw new FormatException("paramater X is invalid format");

            if (!float.TryParse(namedParameters["Y"], out y))
                throw new FormatException("paramater Y is invalid format");

            return new Vector2(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
