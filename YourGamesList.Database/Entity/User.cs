using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entity;

public class User
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required DateTimeOffset DateOfBirth { get; set; }
    [StringLength(256)] public required string Email { get; set; }
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Libsodium max hash size is 128 as of today
    /// </summary>
    [StringLength(128)]
    public required string PasswordHash { get; set; }

    [StringLength(64)] public required string Username { get; set; }

    public ICollection<GamesList> GamesLists { get; } = [];
}