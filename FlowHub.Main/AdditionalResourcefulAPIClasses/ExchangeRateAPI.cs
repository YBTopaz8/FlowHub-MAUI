using Newtonsoft.Json;

namespace FlowHub.Main.AdditionalResourcefulApiClasses;

public class ExchangeRateAPI
{
    public ConvertedRate GetConvertedRate(string UserCurrency, string DestinationCurrency)
    {
        
        try
        {            
            string url_str = $@"https://v6.exchangerate-api.com/v6/f0bfc8e59756851a938ce5cc/pair/{UserCurrency}/{DestinationCurrency}";
            using var webClient = new HttpClient();
            var json = webClient.GetStringAsync(url_str).Result;
            ConvertedRate JsonObject = JsonConvert.DeserializeObject<ConvertedRate>(json);

            return JsonObject;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    public class ConvertedRate
    {
        public string success { get; set; }
        public string result { get; set; }
        public float conversion_rate { get; set; }

        private string time_last_update_utc;
        private string _formattedTime;

        public string TimeLastUpdateUtc
        {
            get => _formattedTime;
            set
            {
                time_last_update_utc = value;
                _formattedTime = ConvertToFormattedDateTime(value);
            }
        }
        
        private string ConvertToFormattedDateTime(string utcDateTime)
        {
            if(DateTime.TryParseExact(utcDateTime, "ddd, dd MMM yyyy HH:mm:ss '+0000'",
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.AdjustToUniversal,
                                   out DateTime parsedDateTime))
            {
                return parsedDateTime.ToLocalTime().ToString("ddd, dd MM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return "Invalid Date";
        }

        
    }


    public class ConversionRate
    {
        public double AUD { get; set; }
        public double BRL { get; set; }
        public double CAD { get; set; }
        public double CNY { get; set; }
        public double EUR { get; set; }
        public double GBP { get; set; }
        public double KES { get; set; }
        public double NGN { get; set; }
        public double TND { get; set; }
        public double USD { get; set; }
        public double XOF { get; set; }
        public double XAF { get; set; }
        public double ZAR { get; set; }
        public double JPY { get; set; }
        public double INR { get; set; }
        public double RUB { get; set; }
        public double NZD { get; set; }
        public double CHF { get; set; }
        public double SEK { get; set; }
        public double NOK { get; set; }
        public double DKK { get; set; }
        public double MXN { get; set; }
        public double SGD { get; set; }
        public double HKD { get; set; }
        public double TRY { get; set; }
        public double BHD { get; set; }
        public double SAR { get; set; }
        public double AED { get; set; }
        public double ARS { get; set; }
        public double ILS { get; set; }
        public double EGP { get; set; }
        public double MYR { get; set; }
        public double THB { get; set; }
        public double IDR { get; set; }
        public double PHP { get; set; }
        public double PLN { get; set; }
        public double CZK { get; set; }
        public double HUF { get; set; }
        public double BGN { get; set; }
        public double PKR { get; set; }
        public double QAR { get; set; } // Qatar Riyal
        public double KRW { get; set; } // South Korean Won
        public double BDT { get; set; } // Bangladesh Taka
        public double VND { get; set; } // Vietnamese Dong
        public double UAH { get; set; } // Ukrainian Hryvnia
        public double PEN { get; set; } // Peruvian Sol
        public double RON { get; set; } // Romanian Leu
        public double CLP { get; set; } // Chilean Peso
        public double COP { get; set; } // Colombian Peso
        public double ZMW { get; set; } // Zambian Kwacha
        public double MUR { get; set; } // Mauritian Rupee
        public double LKR { get; set; } // Sri Lankan Rupee
        public double NPR { get; set; } // Nepalese Rupee
        public double MDL { get; set; } // Moldovan Leu
        public double GHS { get; set; } // Ghanaian Cedi
        public double JMD { get; set; } // Jamaican Dollar
        public double TTD { get; set; } // Trinidad and Tobago Dollar
        public double CRC { get; set; } // Costa Rican Colón
        public double UYU { get; set; } // Uruguayan Peso
        public double DOP { get; set; } // Dominican Peso
        public double KZT { get; set; } // Kazakhstani Tenge
    }
}