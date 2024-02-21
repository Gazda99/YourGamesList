namespace Igdb.Model;

public class PlatformFamily : IIdentifier, IHasChecksum
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
}