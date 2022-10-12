public interface IParseable<TSelf>
    where TSelf : IParseable<TSelf>
{
    [RequiresPreviewFeatures]
    static abstract bool TryParse(string value, out TSelf result);
}

public sealed class GenericConverter<T> : JsonConverter<T>
    where T : class, IParseable<T>
{
    [RequiresPreviewFeatures]
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return T.TryParse(reader.GetString(), out var result)
            ? result
            : throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public sealed class GenericConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // https://source.dot.net/#System.Text.Json/System/Text/Json/Serialization/Converters/Collection/IAsyncEnumerableConverterFactory.cs,20
        // https://source.dot.net/#System.Text.Json/src/libraries/System.Text.Json/Common/ReflectionExtensions.cs,78
        foreach (var interfaceType in typeToConvert.GetInterfaces())
        {
            if (interfaceType.IsGenericType)
            {
                if (interfaceType.GetGenericTypeDefinition() == typeof(IParseable<>))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(GenericConverter<>).MakeGenericType(typeToConvert));
    }
}

public sealed class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new GenericConverterFactory());
    }
}