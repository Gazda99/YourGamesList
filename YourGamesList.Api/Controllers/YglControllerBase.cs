using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Controllers;

public class YglControllerBase : ControllerBase
{
    public InputArguments InputArguments { get; set; } = new();
}