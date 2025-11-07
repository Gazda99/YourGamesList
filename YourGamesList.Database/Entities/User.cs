using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entities;

public class User
{
    public Guid Id { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    [StringLength(50)] public required string Username { get; set; }

    [StringLength(256)] public required string PasswordHash { get; set; }
    [MaxLength(16)] public required byte[] Salt { get; set; }

    public virtual ICollection<GamesList> GamesLists { get; } = [];
}