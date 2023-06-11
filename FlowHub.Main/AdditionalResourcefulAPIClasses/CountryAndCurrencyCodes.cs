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
            { "Congo (Brazzaville)", "XAF" }, // Make sure to specify which Congo
            { "England", "GBP" },
            { "France", "EUR" },
            { "Gabon", "XAF" },
            { "Germany", "EUR" },
            { "Italy", "EUR" },
            { "Kenya", "KES" },
            { "Nigeria", "NGN" },
            { "Senegal", "XOF" },
            { "South Africa", "ZAR" },
            { "Tunisia", "TND" },
            // Additional countries
            { "United States", "USD" },
            { "Japan", "JPY" },
            { "South Korea", "KRW" },
            { "India", "INR" },
            { "Russia", "RUB" },
            { "Turkey", "TRY" },
            { "Switzerland", "CHF" },
            { "Norway", "NOK" },
            { "New Zealand", "NZD" },
            { "Mexico", "MXN" },
            { "Israel", "ILS" },
            { "Saudi Arabia", "SAR" },
            { "Qatar", "QAR" },
            { "Pakistan", "PKR" },
            { "Egypt", "EGP" },
            { "Thailand", "THB" },
            { "Singapore", "SGD" },
            { "Malaysia", "MYR" },
            { "United Arab Emirates", "AED" },
            { "Argentina", "ARS" },
            { "Chile", "CLP" },
            { "Indonesia", "IDR" },
            { "Vietnam", "VND" },
            { "Czech Republic", "CZK" },
            { "Philippines", "PHP" },
            { "Hungary", "HUF" },
            { "Sweden", "SEK" },
            { "Denmark", "DKK" },
            { "Poland", "PLN" },
            { "Croatia", "HRK" },
            { "Romania", "RON" },
            { "Bulgaria", "BGN" }
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
