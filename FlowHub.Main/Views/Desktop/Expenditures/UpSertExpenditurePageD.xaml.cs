using FlowHub.Main.ViewModels.Expenditures;
using System.Diagnostics;
using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class UpSertExpenditurePageD : ContentPage
{
    private readonly UpSertExpenditureVM viewModel;
    double CurrentBalance;
    double InitialExpAmountSpent;
    public UpSertExpenditurePageD(UpSertExpenditureVM vm)
	{
		InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
        InitialExpAmountSpent = viewModel.SingleExpenditureDetails.AmountSpent;
    }

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        //Custom validation
        if (!string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            bool isValid = double.TryParse(e.NewTextValue, out _);
            ((TextField)sender).Text = isValid ? e.NewTextValue : e.OldTextValue;
        }

        CurrentBalance = viewModel.ActiveUser.PocketMoney;

        double total = 0;
        if (string.IsNullOrEmpty(UnitPrice.Text) || string.IsNullOrWhiteSpace(UnitPrice.Text))
        {
        }
        if (!string.IsNullOrEmpty(Qty.Text) || !string.IsNullOrWhiteSpace(Qty.Text))
        {
        }
        _ = double.TryParse(UnitPrice.Text, out double up);
        _ = double.TryParse(Qty.Text, out double qty);

        total = up * qty;

        var diff = total - InitialExpAmountSpent;
        CurrentBalance -= diff;

        viewModel.ResultingBalance = CurrentBalance;
    }

    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
            //Debug.WriteLine("UnitPrice_Focused");
        }
    }
}