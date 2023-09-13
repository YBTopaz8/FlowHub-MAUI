using LiteDB;
using System.ComponentModel;

namespace FlowHub.Models;

public class DebtModel : INotifyPropertyChanged
{
    private bool isPaidCompletely;
    private string? displayText;
  
    [BsonId]
    public string Id { get; set; } 
    public required double Amount { get; set; }
    public DebtType DebtType { get; set; } = DebtType.Lent;
    public required PersonOrOrganizationModel PersonOrOrganization { get; set; }
    public DateTime? Deadline { get; set; } 
    public double AmountPaid { get; set; }
    public string? Currency { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public string? PhoneAddress { get; set; } = string.Empty;
    public DateTime? DatePaidCompletely { get; set; }
    public DateTime AddedDateTime { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public bool IsDeleted { get; set; }
    public string? UserId { get; set; } = string.Empty;
    public string? PlatformModel { get; set; } = string.Empty;
    public List<PaymentAdvances>? PaymentAdvances { get; set; } = Enumerable.Empty<PaymentAdvances>().ToList();

    public string DisplayText
    {
        get => displayText;
        set
        {
            if (displayText != value)
            {
                displayText = value;
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }
    public bool IsPaidCompletely
    {
        get => isPaidCompletely;
        set
        {
            isPaidCompletely = value;
            OnPropertyChanged(nameof(IsPaidCompletely));            
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
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