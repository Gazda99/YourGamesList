using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.ModelBinders;

public class UserInformationTokenModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(UserInformationToken))
        {
            return new BinderTypeModelBinder(typeof(UserInformationTokenModelBinder));
        }

#nullable disable
        return null;
    }
}