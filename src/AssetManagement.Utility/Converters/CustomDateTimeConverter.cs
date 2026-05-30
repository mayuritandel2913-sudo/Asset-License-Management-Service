using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetManagement.Utility.Converters;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _outputFormat = "MM-dd-yyyy HH:mm:ss"; 
    private static readonly string[] AcceptedFormats = { "MM/dd/yyyy", "MM-dd-yyyy" };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value != null)
        {
            if (DateTime.TryParseExact(value, AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime isoDate))
            {
                return isoDate;
            }
        }

        throw new JsonException("Invalid date format. Expected MM-dd-yyyy or ISO 8601 format.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_outputFormat));
    }
}

public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly string _outputFormat = "MM-dd-yyyy HH:mm:ss";
    private static readonly string[] AcceptedFormats = { "MM/dd/yyyy", "MM-dd-yyyy" };

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value)) return null;

        if (DateTime.TryParseExact(value, AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime isoDate))
        {
            return isoDate;
        }

        throw new JsonException("Invalid date format. Expected MM-dd-yyyy or ISO 8601 format.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(_outputFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}