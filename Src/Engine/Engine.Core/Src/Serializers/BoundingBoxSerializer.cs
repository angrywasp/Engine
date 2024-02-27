using System;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;

namespace Engine.Serializers
{
    public class BoundingBoxSerializer : JsonConverter<BoundingBox>
    {
        public string Serialize(BoundingBox value)
        {
            Vector3Serializer s = new Vector3Serializer();
            return s.Serialize(value.Min) + ";" + s.Serialize(value.Max);
        }

        public BoundingBox Deserialize(string value)
        {
            string[] parts = value.Split(';');
            Vector3Serializer s = new Vector3Serializer();
            return new BoundingBox(s.Deserialize(parts[0]), s.Deserialize(parts[1]));
        }

        public override void WriteJson(JsonWriter writer, BoundingBox value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override BoundingBox ReadJson(JsonReader reader, Type objectType, BoundingBox existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
