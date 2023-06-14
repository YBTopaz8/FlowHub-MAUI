using LiteDB;

namespace FlowHub.Models;

public class UsersModel
{
    public List<UsersModel> documents { get; set; }
    [BsonId]
    public string Id { get; set; }
    public string? UserIDOnline { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public double? Savings { get; set; }
    public double PocketMoney { get; set; }
    public double TotalExpendituresAmount { get; set; } = 0;
    public double TotalIncomeAmount { get; set; } = 0;
    public double TotalInDebtAmount { get; set; } = 0;
    public double TotalOutDebtAmount { get; set; } = 0;
    public DateTime DateTimeOfPocketMoneyUpdate { get; set; }
    public string UserCountry { get; set; }
    public string UserCurrency { get; set; }
    public List<TaxModel> Taxes { get; set; }
    public bool RememberLogin { get; set; }
}