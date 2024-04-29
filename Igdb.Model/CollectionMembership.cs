namespace Igdb.Model;

public class CollectionMembership : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Collection> Collection { get; set; }
    public IdentityOrValue<Game> Game { get; set; }
    public string Name { get; set; }
    public IdentityOrValue<CollectionMembershipType> Type { get; set; }
    public string Url { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}