using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class InputCurrencyForPrintPopUpPage : Popup
{
    public InputCurrencyForPrintPopUpPage(string DisplayText, string userCurrency)
    {
        InitializeComponent();
        DisplayAlertText.Text = DisplayText;

       List<string> ListOfCurrencies= new(){ "AUD", "BRL", "CAD", "CNY", "EUR", "GBP", "KES", "NGN", "TND", "USD", "XOF", "XAF", "ZAR"};
       CurrencyPicker.ItemsSource = ListOfCurrencies;
       CurrencyPicker.SelectedItem = userCurrency;
    }

    void OnYesButtonClicked(object sender, EventArgs e)
    {
        string Currency = CurrencyPicker.SelectedItem as string;
        Close(Currency);
    }

    void OnNoButtonClicked(object sender, EventArgs e) => Close("Cancel");
}