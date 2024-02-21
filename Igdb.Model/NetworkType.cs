namespace Igdb.Model;

public class NetworkType : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentitiesOrValues<EventNetwork> EventNetworks { get; set; }
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}