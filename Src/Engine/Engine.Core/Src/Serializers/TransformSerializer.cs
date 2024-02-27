using System;
using Newtonsoft.Json;
using Engine.World;

namespace Engine.Serializers
{
    public class Transform1Serializer : JsonConverter<WorldTransform1>
    {
        public override void WriteJson(JsonWriter writer, WorldTransform1 value, JsonSerializer serializer) =>
            writer.WriteValue(new MatrixSerializer().Serialize(value.Matrix));

        public override WorldTransform1 ReadJson(JsonReader reader, Type objectType, WorldTransform1 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            WorldTransform1.Create(new MatrixSerializer().Deserialize(reader.Value.ToString()));
    }

    public class Transform2Serializer : JsonConverter<WorldTransform2>
    {
        public override void WriteJson(JsonWriter writer, WorldTransform2 value, JsonSerializer serializer) =>
            writer.WriteValue(new MatrixSerializer().Serialize(value.Matrix));

        public override WorldTransform2 ReadJson(JsonReader reader, Type objectType, WorldTransform2 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            WorldTransform2.Create(new MatrixSerializer().Deserialize(reader.Value.ToString()));
    }

    public class Transform3Serializer : JsonConverter<WorldTransform3>
    {
        public override void WriteJson(JsonWriter writer, WorldTransform3 value, JsonSerializer serializer) =>
            writer.WriteValue(new MatrixSerializer().Serialize(value.Matrix));

        public override WorldTransform3 ReadJson(JsonReader reader, Type objectType, WorldTransform3 existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            WorldTransform3.Create(new MatrixSerializer().Deserialize(reader.Value.ToString()));
    }
}