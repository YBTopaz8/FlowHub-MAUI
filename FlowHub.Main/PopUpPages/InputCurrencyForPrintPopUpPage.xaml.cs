using CommunityToolkit.Maui.Views;

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
            "AUD", // Australian Dollar
            "BRL", // Brazilian Real
            "CAD", // Canadian Dollar
            "CNY", // Chinese Yuan
            "EUR", // Euro
            "GBP", // British Pound
            "KES", // Kenyan Shilling
            "NGN", // Nigerian Naira
            "TND", // Tunisian Dinar
            "USD", // US Dollar
            "XOF", // West African CFA Franc
            "XAF", // Central African CFA Franc
            "ZAR", // South African Rand
            "JPY", // Japanese Yen
            "INR", // Indian Rupee
            "RUB", // Russian Ruble
            "NZD", // New Zealand Dollar
            "CHF", // Swiss Franc
            "SEK", // Swedish Krona
            "NOK", // Norwegian Krone
            "DKK", // Danish Krone
            "MXN", // Mexican Peso
            "SGD", // Singapore Dollar
            "HKD", // Hong Kong Dollar
            "TRY", // Turkish Lira
            "BHD", // Bahraini Dinar
            "SAR", // Saudi Riyal
            "AED", // United Arab Emirates Dirham
            "ARS", // Argentine Peso
            "ILS", // Israeli New Shekel
            "EGP", // Egyptian Pound
            "MYR", // Malaysian Ringgit
            "THB", // Thai Baht
            "IDR", // Indonesian Rupiah
            "PHP", // Philippine Peso
            "PLN", // Polish Zloty
            "CZK", // Czech Koruna
            "HUF", // Hungarian Forint
            "BGN", // Bulgarian Lev
            "PKR", // Pakistani Rupee
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