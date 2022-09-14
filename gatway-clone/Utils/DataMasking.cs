using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace gatway_clone.Utils;

public class DataMasking
{
    private static readonly JsonSerializerOptions _settings = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    private static readonly JsonWriterOptions _writerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    public static string MaskJson(object value)
    {
        return MaskJson(value, _settings, _writerOptions);
    }
    private static void MaskJsonElement(Utf8JsonWriter writer, JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (JsonProperty property in jsonElement.EnumerateObject())
            {
                MaskJProperty(writer, property);
            }
            writer.WriteEndObject();
        }
        else if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (JsonElement element in jsonElement.EnumerateArray())
            {
                MaskJsonElement(writer, element);
            }
            writer.WriteEndArray();
        }
        else
        {
            jsonElement.WriteTo(writer);
        }
    }
    private static void MaskJProperty(Utf8JsonWriter writer, JsonProperty jsonProperty)
    {
        if (BaseConfiguration.DataMaskings.ContainsKey(jsonProperty.Name))
        {
            writer.WriteString(jsonProperty.Name, MaskData(jsonProperty.Value.ValueKind == JsonValueKind.String ? jsonProperty.Value.GetString() : jsonProperty.Value.GetRawText(), BaseConfiguration.DataMaskings[jsonProperty.Name]));
        }
        else
        {
            if (jsonProperty.Value.ValueKind != JsonValueKind.Array && jsonProperty.Value.ValueKind != JsonValueKind.Object)
            {
                jsonProperty.WriteTo(writer);
            }
            else
            {
                writer.WritePropertyName(jsonProperty.Name);
                MaskJsonElement(writer, jsonProperty.Value);
            }
        }
    }
    public static string MaskJson(object value, JsonSerializerOptions settings, JsonWriterOptions writerOptions)
    {
        if (value == null) return null;
        string json = value is string ? value as string : JsonSerializer.Serialize(value, settings);
        if (string.IsNullOrEmpty(json)) return json;
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            using MemoryStream ms = new();
            using Utf8JsonWriter writer = new(ms, writerOptions);
            MaskJsonElement(writer, document.RootElement);
            writer.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch { }
        return json;
    }
    public static string MaskData(string data, string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(data)) return string.Empty;
        return pattern.ToUpper() switch
        {
            "MASKALL" => new string('*', data.Length),
            "HIDE" => "MASKED",
            "MASKCARD" => Regex.Replace(data, @"(\d[\s|-]?){12,19}\d", match => match.Value[..6] + new string('X', match.Value.Length - 10) + match.Value[^4..]),
            _ => data
        };
    }
}
