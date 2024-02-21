namespace Igdb.Model;

public class Language : ITimestamps, IIdentifier, IHasChecksum
{
    public string Locale { get; set; }
    public string Name { get; set; }
    public string NativeName { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}