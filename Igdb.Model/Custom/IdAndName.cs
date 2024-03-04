namespace Igdb.Model.Custom;

public class IdAndName : IIdentifier
{
    public string Name { get; set; }
    public long? Id { get; set; }
}