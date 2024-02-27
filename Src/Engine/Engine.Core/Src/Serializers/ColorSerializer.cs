using System;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;

namespace Engine.Serializers
{
    public class ColorSerializer : JsonConverter<Color>
    {
        public string Serialize(Color value) => value.PackedValue.ToString();

        public Color Deserialize(string value) => new Color() { PackedValue = uint.Parse(value) };

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
