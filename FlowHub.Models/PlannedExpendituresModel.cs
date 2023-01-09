using LiteDB;

namespace FlowHub.Models;

public class PlannedExpendituresModel
{
    [BsonId]
    public string Id { get; set; }
    public string Title { get; set; }
    public double TotalAmount { get; set; }
    public string? Comment { get; set; }
    public int NumberOfExpenditures { get; set; }
    public bool IsMonthlyPlanned { get; set; }
    public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;
    public string Currency { get; set; }
    public string? PlatformModel { get; set; }
    public bool UpdateOnSync { get; set; } = false;
    public string UserId { get; set; }
    public List<ExpendituresModel>? Expenditures { get; set; }
}
