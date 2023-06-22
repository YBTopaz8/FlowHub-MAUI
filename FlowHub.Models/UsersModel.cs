using LiteDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowHub.Models;

public class UsersModel : INotifyPropertyChanged
{
    private string username;
    private string? email;
    private double totalExpendituresAmount;
    private double totalIncomeAmount;
    private double totalInDebtAmount;
    private double totalOutDebtAmount;
    private double pocketMoney;
    public List<UsersModel> documents { get; set; }
    [BsonId]
    public string Id { get; set; }
    public string? UserIDOnline { get; set; }
    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }
    public string Password { get; set; }
    public string? Email
    {
        get => email;
        set
        => SetProperty(ref email, value);
    }

    public double? Savings { get; set; }

    public double PocketMoney
    {
        get => pocketMoney;
        set
        => SetProperty(ref pocketMoney, value);
    }

    public double TotalExpendituresAmount {
        get => totalExpendituresAmount;
        set
        => SetProperty(ref totalExpendituresAmount, value);
    }
    public double TotalIncomeAmount {
        get => totalIncomeAmount;
        set => SetProperty(ref totalIncomeAmount, value);

    }
    public double TotalInDebtAmount {
        get => totalInDebtAmount;
        set
        => SetProperty(ref totalInDebtAmount, value);
    }
    public double TotalOutDebtAmount {
        get => totalOutDebtAmount;
        set => SetProperty(ref totalOutDebtAmount, value);

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

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName!);
        return true;
    }
}