using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class InputCurrencyForPrintPopUpPage : Popup
{
    public InputCurrencyForPrintPopUpPage(string DisplayText, string userCurrency)
    {
        InitializeComponent();
        DisplayAlertText.Text = DisplayText;
        //TODO: Pass the user currency in th title of the popup page and remove this display text

        //TODO: Add more currencies
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