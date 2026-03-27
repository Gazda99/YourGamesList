using System;

namespace YourGamesList.Api.ModelBinders;

public class ModelBindingException : Exception
{
    public string ModelName { get; init; } = "";
    public Type? ModelType { get; init; } = null;
    public string ErrorDescription { get; init; } = "";

    public ModelBindingException()
    {
    }
}