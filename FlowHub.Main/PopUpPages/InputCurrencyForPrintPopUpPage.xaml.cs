namespace FlowHub.Main.PopUpPages;

public partial class InputCurrencyForPrintPopUpPage : Popup
{
    public InputCurrencyForPrintPopUpPage(string DisplayText, string UserCurrency)
    {
        InitializeComponent();
        DisplayAlertText.Text = DisplayText;
        //TODO: Pass the user currency in th title of the popup page and remove this display text

        List<string> ListOfCurrencies = new()
        {
            "AED", // United Arab Emirates Dirham
            "AUD", // Australian Dollar
            "ARS", // Argentine Peso
            "BGN", // Bulgarian Lev
            "BHD", // Bahraini Dinar
            "BRL", // Brazilian Real
            "CAD", // Canadian Dollar
            "CHF", // Swiss Franc
            "CNY", // Chinese Yuan
            "CZK", // Czech Koruna
            "DKK", // Danish Krone
            "EGP", // Egyptian Pound
            "EUR", // Euro
            "GBP", // British Pound
            "HKD", // Hong Kong Dollar
            "HUF", // Hungarian Forint
            "IDR", // Indonesian Rupiah
            "ILS", // Israeli New Shekel
            "INR", // Indian Rupee
            "JPY", // Japanese Yen
            "KES", // Kenyan Shilling
            "MXN", // Mexican Peso
            "MYR", // Malaysian Ringgit
            "NGN", // Nigerian Naira
            "NOK", // Norwegian Krone
            "NZD", // New Zealand Dollar
            "PHP", // Philippine Peso
            "PKR", // Pakistani Rupee
            "PLN", // Polish Zloty
            "RUB", // Russian Ruble
            "SAR", // Saudi Riyal
            "SEK", // Swedish Krona
            "SGD", // Singapore Dollar
            "THB", // Thai Baht
            "TND", // Tunisian Dinar
            "TRY", // Turkish Lira
            "USD", // US Dollar
            "XAF", // Central African CFA Franc
            "XOF", // West African CFA Franc
            "ZAR", // South African Rand
        };

        CurrencyPicker.ItemsSource = ListOfCurrencies;
       CurrencyPicker.SelectedItem = UserCurrency;
    }

    void OnYesButtonClicked(object sender, EventArgs e)
    {
        string Currency = CurrencyPicker.SelectedItem as string;
        Close(Currency);
    }

    void OnNoButtonClicked(object sender, EventArgs e) => Close("Cancel");

    private void ChangeCurrencyCheckBox_CheckChanged(object sender, EventArgs e)
    {
        //TODO: Change the size of the popup page and check which is the best size for the popup page
        if (ChangeCurrencyCheckBox.IsChecked)
        {
            PopupPage.Size = new Size((int)Math.Round(PopupPage.Size.Width), 220);
        }
        else
        {
            PopupPage.Size = new Size((int)Math.Round(PopupPage.Size.Width), 180);
        }
    }
}