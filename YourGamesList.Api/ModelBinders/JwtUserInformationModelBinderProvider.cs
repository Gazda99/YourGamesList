using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.ModelBinders;

//TODO: unit tests
public class JwtUserInformationModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(JwtUserInformation))
        {
            return new BinderTypeModelBinder(typeof(JwtUserInformationModelBinder));
        }

#nullable disable
        return null;
    }
}