namespace Igdb.Model;

public class InvolvedCompany : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Company> Company { get; set; }
    public bool? Developer { get; set; }
    public IdentityOrValue<Game> Game { get; set; }
    public bool? Porting { get; set; }
    public bool? Publisher { get; set; }
    public bool? Supporting { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}