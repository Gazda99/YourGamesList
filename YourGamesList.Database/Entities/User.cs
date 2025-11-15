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

    public DateTimeOffset? LastLoginDate { get; set; }

    #region About

    [StringLength(512)] public string Description { get; set; } = string.Empty;
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Country { get; set; } = string.Empty;

    #endregion

    public virtual ICollection<GamesList> GamesLists { get; } = [];
}