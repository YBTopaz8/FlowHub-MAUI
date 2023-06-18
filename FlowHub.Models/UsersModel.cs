using LiteDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowHub.Models;

public class UsersModel : INotifyPropertyChanged
{
    private string? email;
    private double totalExpendituresAmount;
    private double totalIncomeAmount;
    private double totalInDebtAmount;
    private double totalOutDebtAmount;

    public List<UsersModel> documents { get; set; }
    [BsonId]
    public string Id { get; set; }
    public string? UserIDOnline { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Email
    {
        get => email;
        set
        {
            if (email != value)
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    public double? Savings { get; set; }
    public double PocketMoney { get; set; }
    public double TotalExpendituresAmount {
        get => totalExpendituresAmount;
        set
        {
            if (totalExpendituresAmount != value)
            {
                totalExpendituresAmount = value;
                OnPropertyChanged(nameof(TotalExpendituresAmount));
            }
        }
    }
    public double TotalIncomeAmount {
        get => totalIncomeAmount;
        set
        {
            if (totalIncomeAmount != value)
            {
                totalIncomeAmount = value;
                OnPropertyChanged(nameof(TotalIncomeAmount));
            }
        }
    }
    public double TotalInDebtAmount {
        get => totalInDebtAmount;
        set
        {
            if (totalInDebtAmount != value)
            {
                totalInDebtAmount = value;
                OnPropertyChanged(nameof(TotalInDebtAmount));
            }
        }
    }
    public double TotalOutDebtAmount {
        get => totalOutDebtAmount;
        set
        {
            if (totalOutDebtAmount != value)
            {
                totalOutDebtAmount = value;
                OnPropertyChanged(nameof(TotalOutDebtAmount));
            }
        }
    }
    public DateTime DateTimeOfPocketMoneyUpdate { get; set; }
    public string UserCountry { get; set; } = null!;
    public string UserCurrency { get; set; } = null!;
    public List<TaxModel> Taxes { get; set; } = null!;
    public bool RememberLogin { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}