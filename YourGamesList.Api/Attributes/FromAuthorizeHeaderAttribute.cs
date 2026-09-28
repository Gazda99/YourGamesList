using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromAuthorizeHeaderAttribute : FromHeaderAttribute
{
    public FromAuthorizeHeaderAttribute()
    {
        Name = "Authorization";
    }
}