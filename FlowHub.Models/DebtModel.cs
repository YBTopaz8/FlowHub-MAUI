using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FlowHub.Models;

public partial class DebtModel : ObservableObject
{
    private bool isPaidCompletely;
    private string? displayText;
  
    [BsonId]
    public string Id { get; set; }
    [ObservableProperty]
    public double amount ;
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
    private ObservableCollection<InstallmentPayments>? paymentAdvances { get; set; } = new ObservableCollection<InstallmentPayments>();

    public ObservableCollection<InstallmentPayments> PaymentAdvances
    {
        get => paymentAdvances;
        set
        {
            if (paymentAdvances != value)
            {
                paymentAdvances = value;
                OnPropertyChanged(nameof(PaymentAdvances));
            }
        }
    }

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
public class InstallmentPayments
{
    [BsonId]
    public string Id { get; set; }
    public required double AmountPaid { get; set; }
    public string? ReasonForOptionalPayment { get; set; }
    public required DateTime DatePaid { get; set; } = DateTime.Now;
}
public class PersonOrOrganizationModel
{
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}