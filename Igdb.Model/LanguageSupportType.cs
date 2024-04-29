namespace Igdb.Model;

public class LanguageSupportType : ITimestamps, IIdentifier, IHasChecksum
{
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}