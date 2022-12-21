using LiteDB;

namespace FlowHub.Models;

public class ExpendituresModel
{

    [BsonId]
    public string Id { get; set; }
    public DateTime DateSpent { get; set; }
    public double AmountSpent { get; set; }
    public string? Reason { get; set; }
    public bool IncludeInReport { get; set; } = true;
    public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }
    public string Currency { get; set; }
    public string? PlatformModel { get; set; }
    public bool UpdateOnSync { get; set; } = false;
    public string UserId { get; set; }
}
