namespace Igdb.Model;

public class LanguageSupportType : ITimestamps, IIdentifier, IHasChecksum
{
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}