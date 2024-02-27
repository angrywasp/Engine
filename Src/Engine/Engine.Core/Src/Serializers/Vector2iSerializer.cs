using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AngryWasp.Helpers;
using Microsoft.Xna.Framework;

namespace Engine.Serializers
{
    public class Vector2iSerializer : JsonConverter<Vector2i>
    {
        public string Serialize(Vector2i value) => value.ToString();

        public Vector2i Deserialize(string value)
        {
            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 2)
                throw new FormatException("paramater is invalid format");

            int x, y;

            if (!int.TryParse(namedParameters["X"], out x))
                throw new FormatException("paramater is invalid format");

            if (!int.TryParse(namedParameters["Y"], out y))
                throw new FormatException("paramater is invalid format");

            return new Vector2i(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2i value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Vector2i ReadJson(JsonReader reader, Type objectType, Vector2i existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
