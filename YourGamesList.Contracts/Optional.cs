using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YourGamesList.Contracts;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly struct Optional<T>
{
    private readonly T? _value;

    private Optional(T? value)
    {
        _value = value;
        HasValue = true;
    }

    public bool HasValue { get; }

    public T? Value =>
        HasValue
            ? _value
            : throw new InvalidOperationException("An omitted optional value has no value.");

    public static Optional<T> Some(T? value) => new(value);
}

public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType &&
        typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }

    private class OptionalJsonConverter<TValue> : JsonConverter<Optional<TValue>>
    {
        public override bool HandleNull => true;

        public override Optional<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return Optional<TValue>.Some(default);
            }

            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            return Optional<TValue>.Some(value);
        }

        public override void Write(Utf8JsonWriter writer, Optional<TValue> value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}