using Newtonsoft.Json.Serialization;

namespace YourGamesList.Common;

public static class JsonConvertSerializers
{
    public static readonly JsonSerializerSettings SnakeCaseNaming = new JsonSerializerSettings()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public static readonly JsonSerializerSettings CamelCase = new JsonSerializerSettings()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };
}