using Newtonsoft.Json;

namespace FlowHub.Main.AdditionalResourcefulApiClasses;

public class ExchangeRateAPI
{
    public ConvertedRate GetConvertedRate(string UserCurrency, string DestinationCurrency)
    {
        //string url_str = $"https://api.exchangerate.host/latest?places=5&&base={CurrencySymbol}"; string to get base
        string url_str = $"https://api.exchangerate.host/convert?from={UserCurrency}&to={DestinationCurrency}";

        using var webClient = new HttpClient();
        var json = webClient.GetStringAsync(url_str).Result;
        ConvertedRate JsonObject = JsonConvert.DeserializeObject<ConvertedRate>(json);

        return JsonObject;
    }

    public class API_Obj
    {
        public string success { get; set; }
        public DateTime date { get; set; }
        public ConversionRate rates { get; set; }

    }

    public class ConvertedRate
    {
        public string success { get; set; }
        public DateTime date { get; set; }
        public double result { get; set; }
    }

    public class ConversionRate
    {
        public double CAD { get; set; }
        public double EUR { get; set; }
        public double TND { get; set; }
        public double USD { get; set; }
        public double XAF { get; set; }
        public double ZAR { get; set; }
    }
}