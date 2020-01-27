using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Json.Converters
{
    public class DateTimeFormatConverter : DateTimeConverterBase
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string RFC3339 = "yyyy-MM-dd'T'HH:mm:ssZ";

        public string Format { get; set; } = RFC3339;

        public DateTimeFormatConverter() { }

        public DateTimeFormatConverter(string format)
        {
            Format = format;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dateTime)
            {
                var date = (DateTime)value;
                if (string.IsNullOrEmpty(Format))
                {
                    writer.WriteValue(date.ToString());
                }
                else
                {
                    writer.WriteValue(date.ToString(Format));
                }
            }
            else
            {
                throw new JsonSerializationException("Expected date object value.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                if (!nullable)
                {
                    throw new JsonSerializationException($"Cannot convert null value to {objectType}.");
                }

                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                DateTime date;
                var text = (string)reader.Value;

                if (string.IsNullOrEmpty(Format))
                {
                    if (!DateTime.TryParse(text, out date))
                    {
                        throw new JsonSerializationException($"Cannot convert invalid value to {objectType}.");
                    }
                }
                else
                {
                    var provider = CultureInfo.InvariantCulture;
                    if (!DateTime.TryParseExact(text, Format, provider, DateTimeStyles.None, out date))
                    {
                        throw new JsonSerializationException($"Cannot convert invalid value to {objectType}.");
                    }
                }

                return date;
            }
            throw new JsonSerializationException($"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
        }
    }
}
