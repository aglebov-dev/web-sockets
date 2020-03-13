using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WSServer
{
    internal static class Common
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; }

        static Common()
        {
            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new TimeSpanConverter());

            JsonSerializerOptions = jsonOptions;
        }

        private class TimeSpanConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return TimeSpan.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
