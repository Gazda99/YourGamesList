namespace YourGamesList.Services.Igdb.Model;

public class IgdbWebhook
{
    public long Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public long Category { get; set; }
    public int SubCategory { get; set; }
    public bool Active { get; set; }
    public int NumberOfRetries { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}