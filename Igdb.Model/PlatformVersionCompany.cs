namespace Igdb.Model;

public class PlatformVersionCompany : IIdentifier, IHasChecksum
{
    public string Comment { get; set; }
    public IdentityOrValue<Company> Company { get; set; }
    public bool? Developer { get; set; }
    public bool? Manufacturer { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
}