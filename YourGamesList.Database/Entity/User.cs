using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entity;

public class User
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required DateOnly DateOfBirth { get; set; }
    [StringLength(512)] public string? Description { get; set; }
    [StringLength(256)] public required string Email { get; set; }
    public DateTimeOffset? ExpiryAt { get; set; }
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Libsodium max hash size is 128 as of today
    /// </summary>
    [StringLength(128)]
    public required string PasswordHash { get; set; }
    
    public required UserStatus UserStatus { get; set; }

    [StringLength(64)] public required string Username { get; set; }

    public virtual ICollection<GamesList> GamesLists { get; } = [];
}