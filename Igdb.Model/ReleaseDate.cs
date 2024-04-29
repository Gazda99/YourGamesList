using Newtonsoft.Json;

namespace Igdb.Model;

public class ReleaseDate : ITimestamps, IIdentifier, IHasChecksum
{
    public ReleaseDateCategory? Category { get; set; }
    public long? Date { get; set; }
    public IdentityOrValue<Game> Game { get; set; }
    public string Human { get; set; }

    [JsonProperty("m")] public int? Month { get; set; }

    public IdentityOrValue<Platform> Platform { get; set; }
    public ReleaseDateRegion? Region { get; set; }
    public IdentityOrValue<ReleaseDateStatus> Status { get; set; }

    [JsonProperty("y")] public int? Year { get; set; }

    public string Checksum { get; set; }
    public long? Id { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}