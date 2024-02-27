using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AngryWasp.Helpers;
using Microsoft.Xna.Framework;

namespace Engine.Serializers
{
    public class RectangleSerializer : JsonConverter<Rectangle>
    {
        public string Serialize(Rectangle value) => value.ToString();

        public Rectangle Deserialize(string value)
        {
            Dictionary<string, string> namedParameters;
            if (!StringHelper.TryGetTypeParams(value, out namedParameters))
                throw new FormatException("paramater is invalid format");

            if (namedParameters.Count != 4)
                throw new FormatException("paramater is invalid format");

            int x, y, w, h;

            if (!int.TryParse(namedParameters["X"], out x))
                throw new FormatException("paramater X is invalid format");

            if (!int.TryParse(namedParameters["Y"], out y))
                throw new FormatException("paramater Y is invalid format");

            if (!int.TryParse(namedParameters["Width"], out w))
                throw new FormatException("paramater Width is invalid format");

            if (!int.TryParse(namedParameters["Height"], out h))
                throw new FormatException("paramater Height is invalid format");

            return new Rectangle(x, y, w, h);
        }

        public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
