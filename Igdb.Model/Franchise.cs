namespace Igdb.Model;

public class Franchise : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentitiesOrValues<Game> Games { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Url { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}