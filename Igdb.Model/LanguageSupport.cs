namespace Igdb.Model;

public class LanguageSupport : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Game> Game { get; set; }
    public IdentityOrValue<Language> Language { get; set; }
    public IdentityOrValue<LanguageSupportType> LanguageSupportType { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}