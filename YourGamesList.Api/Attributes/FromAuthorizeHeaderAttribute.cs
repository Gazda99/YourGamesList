using System;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromAuthorizeHeaderAttribute : FromHeaderAttribute
{
    public FromAuthorizeHeaderAttribute()
    {
        Name = "Authorization";
    }
}