using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace YourGamesList.Database.Entity;

[ExcludeFromCodeCoverage]
public class EmailVerification
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required DateTimeOffset ExpiryAt { get; set; }
    [StringLength(128)] public required string Token { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}