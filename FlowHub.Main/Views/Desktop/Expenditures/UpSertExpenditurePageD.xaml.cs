using FlowHub.Main.ViewModels.Expenditures;

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
}