using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.ModelBinders;

namespace YourGamesList.Api.UnitTests.ModelBinders;

public class JwtUserInformationModelBinderProviderTests
{
    [Test]
    public void GetBinder_OnCorrectModel_ReturnsCorrectBinder()
    {
        //ARRANGE
        var context = Substitute.For<ModelBinderProviderContext>();
        var modelMetadata = Substitute.For<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(JwtUserInformation)));
        context.Metadata.Returns(modelMetadata);
        var modelBinderProvider = new JwtUserInformationModelBinderProvider();

        //ACT
        var binder = modelBinderProvider.GetBinder(context);

        //ASSERT
        Assert.That(binder, Is.Not.Null);
        Assert.That(binder, Is.TypeOf<BinderTypeModelBinder>());
    }

    [Test]
    public void GetBinder_OnContextNull_ThrowsArgumentNullException()
    {
        //ARRANGE
        ModelBinderProviderContext? context = null;
        var modelBinderProvider = new JwtUserInformationModelBinderProvider();

        //ACT - ASSERT
        Assert.Throws<ArgumentNullException>(() => modelBinderProvider.GetBinder(context));
    }
}