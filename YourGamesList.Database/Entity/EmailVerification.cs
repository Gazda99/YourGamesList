using System;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entity;

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