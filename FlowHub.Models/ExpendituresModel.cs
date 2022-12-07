using LiteDB;

namespace FlowHub.Models;

public class ExpendituresModel
{

    [BsonId]
    public string Id { get; set; }
    public DateTime DateSpent { get; set; }
    public double AmountSpent { get; set; }
    public string? Reason { get; set; }
    public DateTime AddedDateTime { get; set; } = DateTime.Now;
    public DateTime UpdatedDateTime { get; set; } = DateTime.Now;
    public string Currency { get; set; }
    public bool UpdateOnSync { get; set; } = false;
    public string? PlatformModel { get; set; }

    public string UserId { get; set; }
}
