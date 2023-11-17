namespace FlowHub.Main.PopUpPages;

public partial class UpSertInstallmentPayment : Popup
{
    public UpSertDebtVM viewModel;
    public UpSertInstallmentPayment(UpSertDebtVM vm)
	{
		InitializeComponent();
        BindingContext = vm;
        viewModel = vm;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

#if WINDOWS
        Size = new Size(350, 320);
#endif
    }

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ClosePopUp) && viewModel.ClosePopUp)
        {
            viewModel.ClosePopUp = false;
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (viewModel.ThisPopUpResult == PopupResult.Cancel)
            {
                await CloseAsync(new PopUpCloseResult() { Data = "CancelData", Result = viewModel.ThisPopUpResult });
            }
            await CloseAsync(new PopUpCloseResult() { Data = "OK Data", Result = viewModel.ThisPopUpResult });
        }
    }

    private void TextField_Focused(object sender, FocusEventArgs e)
    {
        if(AmountPaid.Text == "0")
        {
            AmountPaid.Text = "";
        }
    }
}