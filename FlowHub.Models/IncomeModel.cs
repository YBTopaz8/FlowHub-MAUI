using LiteDB;

namespace FlowHub.Models;

public class IncomeModel
{
    [BsonId]
    public string Id { get; set; }
    public DateTime DateReceived { get; set; }
    public double AmountReceived { get; set; }
    public string? Reason { get; set; }
    public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;
    public string Currency { get; set; }
    public string? PlatformModel { get; set; }
    public bool IsDeleted { get; set; }
    public string UserId { get; set; }
}
