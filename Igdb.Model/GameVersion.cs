namespace Igdb.Model;

public class GameVersion : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentitiesOrValues<GameVersionFeature> Features { get; set; }
    public IdentityOrValue<Game> Game { get; set; }
    public IdentitiesOrValues<Game> Games { get; set; }
    public string Url { get; set; }

    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}