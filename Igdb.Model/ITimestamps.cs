namespace Igdb.Model;

public interface ITimestamps
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}