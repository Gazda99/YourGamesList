namespace Igdb.Model;

public class ReleaseDateStatus : ITimestamps, IIdentifier, IHasChecksum
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}