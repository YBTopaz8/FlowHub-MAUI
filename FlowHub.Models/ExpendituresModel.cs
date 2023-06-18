using LiteDB;
using System.ComponentModel;

namespace FlowHub.Models;

public class ExpendituresModel
{
    [BsonId]
    public string Id { get; set; }
    public DateTime DateSpent { get; set; }
    public double UnitPrice { get; set; }
    public double Quantity { get; set; } = 1;
    public double AmountSpent { get; set; }
    public List<TaxModel>? Taxes { get; set; }
    public string? Reason { get; set; }
    public bool IsPurchase { get; set; } = true; //if false then it's a Payment
    public bool IncludeInReport { get; set; } = true;
    public DateTime AddedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDateTime { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ExpenditureCategory Category { get; set; }
    public string? PlatformModel { get; set; }
    public bool UpdateOnSync { get; set; } = false;
    public string UserId { get; set; }
}

public enum ExpenditureCategory
{
    [Description("Transportation")]
    Transportation,
    [Description("Food and Dining")]
    FoodAndDining,
    [Description("Entertainment")]
    Entertainment,
    [Description("Shopping")]
    Shopping,
    [Description("Utilities")]
    Utilities,
    [Description("Rent")]
    Rent,
    [Description("Health and Fitness")]
    HealthAndFitness,
    [Description("Travel")]
    Travel,
    [Description("Education")]
    Education,
    [Description("Gifts and Transfers")]
    GiftsAndDonations,
    [Description("Other")]
    Other
    //[Description("Taxes")]
    //Taxes,
    //[Description("Insurance")]
    //Insurance,
    //[Description("Fees and Charges")]
    //FeesAndCharges,
    //[Description("Personal Care")]
    //PersonalCare,
    //[Description("Mortgage")]
    //Mortgage,

}