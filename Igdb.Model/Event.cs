namespace Igdb.Model;

public class Event : ITimestamps, IIdentifier, IHasChecksum
{
    public string Description { get; set; }
    public long? EndTime { get; set; }
    public IdentityOrValue<EventLogo> EventLogo { get; set; }
    public IdentitiesOrValues<EventNetwork> EventNetwork { get; set; }
    public IdentitiesOrValues<Game> Games { get; set; }
    public string LiveStreamUrl { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public long? StartTime { get; set; }
    public string TimeZone { get; set; }
    public IdentitiesOrValues<GameVideo> Videos { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}