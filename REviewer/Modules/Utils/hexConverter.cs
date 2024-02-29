using Newtonsoft.Json;

namespace REviewer.Modules.Utils
{
    public class HexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(int);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            int.Parse(((string)reader.Value).Substring(2), System.Globalization.NumberStyles.HexNumber);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue("0x" + ((int)value).ToString("X"));
    }
}
