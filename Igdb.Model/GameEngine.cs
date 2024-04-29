namespace Igdb.Model;

public class GameEngine : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentitiesOrValues<Company> Companies { get; set; }

    public string Description { get; set; }
    public IdentityOrValue<GameEngineLogo> Logo { get; set; }

    public string Name { get; set; }

    public IdentitiesOrValues<Platform> Platforms { get; set; }

    public string Slug { get; set; }
    public string Url { get; set; }

    public string Checksum { get; set; }
    public long? Id { get; set; }

    public long? CreatedAt { get; set; }

    public long? UpdatedAt { get; set; }
}