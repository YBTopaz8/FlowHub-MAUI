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
    public ExpenditureCategory Category { get; set; } = ExpenditureCategory.None;
    public string? PlatformModel { get; set; }
    public bool UpdateOnSync { get; set; } = false;
    public bool IsDeleted { get; set; }
    public string UserId { get; set; }
}

public enum ExpenditureCategory
{
    [Description("Education")]
    Education = 0,

    [Description("Entertainment")]
    Entertainment = 1,

    [Description("Food")]
    Food = 2,

    [Description("Health")]
    Health = 3,

    [Description("None")]
    None = 4,

    [Description("Other")]
    Other = 5,

    [Description("Rent")]
    Rent = 6,

    [Description("Shopping")]
    Shopping = 7,

    [Description("Transfers")]
    Transfers = 8,

    [Description("Transportation")]
    Transportation = 9,

    [Description("Travel")]
    Travel = 10,

    [Description("Utilities")]
    Utilities = 11
}

public static class ExpenditureCategoryDescriptions
{
    public static List<string> Descriptions { get; }
    static ExpenditureCategoryDescriptions() => Descriptions = GetEnumDescriptions<ExpenditureCategory>();

    private static List<string>? GetEnumDescriptions<T>()
    {
        Type type = typeof(T);
        if (!type.IsEnum)
        {
            throw new ArgumentException("Type must be an enum");
        }

        var descriptions = new List<string>();
        foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (Attribute.IsDefined(field, typeof(DescriptionAttribute)))
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                descriptions.Add(attribute?.Description ?? string.Empty);
            }
        }

        return descriptions;
    }
}