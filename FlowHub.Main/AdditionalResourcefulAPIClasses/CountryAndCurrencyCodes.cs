using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.AdditionalResourcefulAPIClasses;

public class CountryAndCurrencyCodes
{

    Dictionary<string, string> CountryAndCurrency; 
     readonly List<string> CountryNames= new();

    public Dictionary<string, string> LoadDictionaryWithCountryAndCurrency()
    {
        CountryAndCurrency = new()
        {
            { "Australia", "AUD" },
            { "Benin", "XOF" },
            { "Brazil", "BRL" },
            { "Burkina Faso", "XOF" },
            { "Cameroon", "XAF" },
            { "Canada", "CAD" },
            { "China", "CNY" },
            { "Congo", "XAF" },
            { "England", "GBP" },
            { "France", "EUR" },
            { "Gabon", "XAF" },
            { "Germany", "EUR" },
            { "Italy", "EUR" },
            { "Kenya", "KES" },
            { "Nigeria", "NGN" },
            { "Senegal", "XOF" },
            { "South Africa", "ZAR" },
            { "Tunisia", "TND" }
        };

        return CountryAndCurrency;
    }

    public List<string> GetCountryNames()
    {
        
        var dict = LoadDictionaryWithCountryAndCurrency();

        foreach (var item in dict)
        {
            CountryNames.Add(item.Key);
        }
        return CountryNames;
    }
    
}
