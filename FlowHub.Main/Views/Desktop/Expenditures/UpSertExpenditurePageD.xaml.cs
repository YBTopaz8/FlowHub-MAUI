using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class UpSertExpenditurePageD : ContentPage
{
    private readonly UpSertExpenditureVM viewModel;
    public UpSertExpenditurePageD(UpSertExpenditureVM vm)
	{
		InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
        if (viewModel.IsAddTaxesChecked)
        {
            AddTaxCheckBox.IsChecked = true;
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        AddTaxCheckBox.IsChecked = false;
    }

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        var s = sender as TextField;
        if (s.Text?.Length == 0)
        {
            viewModel.SingleExpenditureDetails.UnitPrice = 0;
        }

        viewModel.UnitPriceOrQuantityChanged();
    }

    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
        }
    }

    private void TaxCheckbox_CheckChanged(object sender, EventArgs e)
    {
        var s = sender as InputKit.Shared.Controls.CheckBox;
        var tax = (TaxModel)s.BindingContext;
        if (s.IsChecked)
        {
            tax.IsChecked = true;
            viewModel.AddTax(tax);
        }
        else
        {
            tax.IsChecked = false;

            viewModel.RemoveTax(tax);
        }
    }

    private void AddTax_CheckChanged(object sender, EventArgs e)
    {
        if (AddTaxCheckBox.IsChecked)
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                viewModel.AddTax(tax);
            }
        }
        else
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                viewModel.RemoveTax(tax);
            }
        }
    }
}