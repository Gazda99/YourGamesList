namespace Igdb.Model;

public class Region : ITimestamps, IIdentifier, IHasChecksum
{
    public string Category { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}