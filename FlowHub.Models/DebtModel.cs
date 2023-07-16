using LiteDB;

namespace FlowHub.Models;

public class DebtModel
{
    [BsonId]
    public string Id { get; set; }
    public required double Amount { get; set; }
    public required PersonOrOrganizationModel PersonOrOrganization { get; set; }
    public DateTime? Deadline { get; set; }
    public double AmountPaid { get; set; }
    public string Currency { get; set; }
    public DateTime? DatePaidCompletely { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPaidCompletely { get; set; }
    public string? Notes { get; set; }
    public string UserId { get; set; }
}