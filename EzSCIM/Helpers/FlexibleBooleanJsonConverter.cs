using System.Text.Json;
using System.Text.Json.Serialization;

namespace EzSCIM.Helpers;

/// <summary>
/// JSON converter that handles flexible boolean deserialization.
/// Accepts both boolean values (true/false) and string representations ("true"/"false").
/// </summary>
public class FlexibleBooleanJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                {
                    var stringValue = reader.GetString();
                    if (bool.TryParse(stringValue, out var result))
                    {
                        return result;
                    }
                    throw new JsonException($"Unable to convert \"{stringValue}\" to boolean.");
                }
            case JsonTokenType.Number:
                {
                    if (reader.TryGetInt32(out var intValue))
                    {
                        return intValue != 0;
                    }
                    break;
                }
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing boolean.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}

