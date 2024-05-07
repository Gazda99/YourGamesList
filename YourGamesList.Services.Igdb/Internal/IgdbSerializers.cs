using Igdb.Model.Serialization;
using Newtonsoft.Json.Serialization;

namespace YourGamesList.Services.Igdb.Internal;

internal static class IgdbSerializers
{
    public static readonly JsonSerializerSettings IgdbResponseSerializer = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter>
        {
            new IdentityConverter()
        },
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };
}