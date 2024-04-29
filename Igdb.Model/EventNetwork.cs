namespace Igdb.Model;

public class EventNetwork : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Event> Event { get; set; }
    public IdentityOrValue<NetworkType> NetworkType { get; set; }
    public string Url { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}