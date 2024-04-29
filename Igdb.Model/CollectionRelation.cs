namespace Igdb.Model;

public class CollectionRelation : ITimestamps, IIdentifier, IHasChecksum
{
    public IdentityOrValue<Collection> ChildCollection { get; set; }
    public IdentityOrValue<Collection> ParentCollection { get; set; }
    public IdentityOrValue<CollectionRelationType> Type { get; set; }
    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}