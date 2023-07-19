using LiteDB;

namespace FlowHub.Models;

public class DebtModel
{
    public DebtModel()
    {
        DebtType = DebtType.Lent;
    }
    [BsonId]
    public string? Id { get; set; }
    public required double Amount { get; set; }
    public DebtType DebtType { get; set; }
    public required PersonOrOrganizationModel PersonOrOrganization { get; set; }
    public DateTime? Deadline { get; set; }
    public double AmountPaid { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public string? PhoneAddress { get; set; }
    public DateTime? DatePaidCompletely { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPaidCompletely { get; set; }
    public string? UserId { get; set; }
    public string? PlatformModel { get; set; }
    public List<PaymentAdvances>? PaymentAdvances { get; set; }
}
public enum DebtType
{
    Borrowed, // User has borrowed money (owes money)
    Lent      // User has lent money (is owed money)
}

public class PaymentAdvances
{
    public double AmountPaid { get; set; }
    public DateTime DatePaid { get; set; }
}
public class PersonOrOrganizationModel
{
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}