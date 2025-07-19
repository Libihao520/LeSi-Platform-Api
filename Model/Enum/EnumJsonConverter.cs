using System.Text.Json;
using System.Text.Json.Serialization;

namespace Model.Enum;

public class EnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, System.Enum
{
    private static readonly Dictionary<string, TEnum> ChineseToEnumMap =
        new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase)
        {
            { "超级管理员", (TEnum)(object)AuthorizeRoleName.Administrator },
            { "编辑用户", (TEnum)(object)AuthorizeRoleName.Editor },
            { "普通用户", (TEnum)(object)AuthorizeRoleName.Ordinary },
        };

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("JSON token was not a string.");
        }

        var enumString = reader.GetString();

        if (ChineseToEnumMap.TryGetValue(enumString, out TEnum result))
        {
            return result;
        }

        if (System.Enum.TryParse<TEnum>(enumString, true, out TEnum parsedResult))
        {
            return parsedResult;
        }

        throw new JsonException($"Could not convert string '{enumString}' to enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}