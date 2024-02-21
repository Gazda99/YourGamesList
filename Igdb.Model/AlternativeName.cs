namespace Igdb.Model;

public class AlternativeName : IIdentifier, IHasChecksum
{
    public string Comment { get; set; }
    public IdentityOrValue<Game> Game { get; set; }
    public string Name { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
}