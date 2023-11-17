using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class UpSertExpenditureBottomSheet : BottomSheetView
{
    private readonly UpSertExpenditureVM viewModel;

    public UpSertExpenditureBottomSheet(UpSertExpenditureVM vm)
	{
		InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }

    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
        }
    }

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (UnitPrice.Text.Length == 0)
        {
            viewModel.SingleExpenditureDetails.UnitPrice = 0;
        }

        viewModel.UnitPriceOrQuantityChanged();
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        this.IsPresented = false;
    }
}