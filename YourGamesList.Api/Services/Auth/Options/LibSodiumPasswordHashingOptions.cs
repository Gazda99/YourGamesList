using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class LibSodiumPasswordHashingOptions
{
    public const string SectionName = "LibSodiumPasswordHashing";

    public ulong OpsLimit { get; set; } = 2;
    public nuint MemoryLimit { get; set; } = 19_922_944;
}

public class LibSodiumPasswordHashingOptionsValidator : AbstractValidator<LibSodiumPasswordHashingOptions>
{
    public LibSodiumPasswordHashingOptionsValidator()
    {
        // Set by crypto_pwhash_opslimit_min
        RuleFor(x => x.OpsLimit).GreaterThanOrEqualTo((ulong) 1);
        // Set by crypto_pwhash_memlimit_min
        RuleFor(x => x.MemoryLimit).GreaterThanOrEqualTo((nuint) 8192);
    }
}