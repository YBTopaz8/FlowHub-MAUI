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
    public DateTime? DatePaidCompletely { get; set; }
    public DateTime AddedDateTime { get; set; }
    public string? Notes { get; set; }
}