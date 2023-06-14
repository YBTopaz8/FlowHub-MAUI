using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Models;
using System.Diagnostics;
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
        viewModel.PageLoadedCommand.Execute(null);
    }

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
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
            viewModel.ApplyTaxes();
        }
        else
        {
            tax.IsChecked = false;
            viewModel.RemoveTax(tax);
            viewModel.UnApplyTaxes();
        }
    }

    private void AddTax_CheckChanged(object sender, EventArgs e)
    {
        if (AddTaxCheckBox.IsChecked)
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                tax.IsChecked = true;                
                viewModel.AddTax(tax);
            }
            viewModel.ApplyTaxes();
        }
        else
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                tax.IsChecked = false;
                viewModel.RemoveTax(tax);
            }
            viewModel.UnApplyTaxes();
        }
    }
}