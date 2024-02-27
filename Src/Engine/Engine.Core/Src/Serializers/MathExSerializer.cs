using System;
using Newtonsoft.Json;
using AngryWasp.Math;

namespace Engine.Serializers
{
    public class DegreeSerializer : JsonConverter<Degree>
    {
        public string Serialize(Degree value) => value.ToString();

        public Degree Deserialize(string value) => Degree.Parse(value);

        public override void WriteJson(JsonWriter writer, Degree value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Degree ReadJson(JsonReader reader, Type objectType, Degree existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }

    public class RadianSerializer : JsonConverter<Radian>
    {
        public string Serialize(Radian value) => value.ToString();

        public Radian Deserialize(string value) => Radian.Parse(value);

        public override void WriteJson(JsonWriter writer, Radian value, JsonSerializer serializer) =>
            writer.WriteValue(Serialize(value));

        public override Radian ReadJson(JsonReader reader, Type objectType, Radian existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            Deserialize(reader.Value.ToString());
    }
}
