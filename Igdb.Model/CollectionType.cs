namespace Igdb.Model;

public class CollectionType : ITimestamps, IHasChecksum
{
    public string Description { get; set; }
    public long? Id { get; set; }
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}