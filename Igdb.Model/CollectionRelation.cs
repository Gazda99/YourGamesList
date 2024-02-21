namespace Igdb.Model;

public class CollectionRelation : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Collection> ChildCollection { get; set; }
    public IdentityOrValue<Collection> ParentCollection { get; set; }
    public IdentityOrValue<CollectionRelationType> Type { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}