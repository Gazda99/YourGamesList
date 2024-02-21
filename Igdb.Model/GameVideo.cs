namespace Igdb.Model;

public class GameVideo : IIdentifier, IHasChecksum
{
    public IdentityOrValue<Game> Game { get; set; }
    public string Name { get; set; }
    public string VideoId { get; set; }

    public string Checksum { get; set; }
    public long? Id { get; set; }
}