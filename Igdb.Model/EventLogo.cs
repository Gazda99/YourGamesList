namespace Igdb.Model;

public class EventLogo : ITimestamps, IIdentifier, IHasChecksum
{
    public bool? AlphaChannel { get; set; }
    public bool? Animated { get; set; }
    public IdentityOrValue<Event> Event { get; set; }
    public int? Height { get; set; }
    public string ImageId { get; set; }
    public string Url { get; set; }
    public int? Width { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}