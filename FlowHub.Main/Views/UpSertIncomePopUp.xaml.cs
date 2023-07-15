using System.ComponentModel;
using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views;

public partial class UpSertIncomePopUp : Popup
{
	readonly UpSertIncomeVM viewModel;
	public UpSertIncomePopUp(UpSertIncomeVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext = vm;
        viewModel.PageLoaded();

#if WINDOWS
        this.Size = new Size(400, 350);
#endif
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ClosePopUp) && viewModel.ClosePopUp)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (viewModel.ThisPopUpResult == PopupResult.Cancel)
            {
                Close(new PopUpCloseResult() { Data = null, Result = PopupResult.Cancel });
                return;
            }
            Close(new PopUpCloseResult() { Data = viewModel.SingleIncomeDetails, Result = viewModel.ThisPopUpResult });
        }
    }

    private void AmountReceived_TextChanged(object sender, TextChangedEventArgs e)
    {
        var s = sender as TextField;
        if(s.Text?.Length == 0)
        {
            viewModel.SingleIncomeDetails.AmountReceived = 0;
        }
        viewModel.AmountReceivedChanged();
    }

    private void AmountReceived_Focused(object sender, FocusEventArgs e)
    {
        if (AmountReceived.Text == "0")
        {
            AmountReceived.Text = "";
        }
    }
}