namespace FlowHub.Main.Views.Desktop.Debts;

public partial class UpSertDebtPopUp : Popup
{
    private readonly UpSertDebtVM viewModel;

    public UpSertDebtPopUp(UpSertDebtVM vm)
	{
		InitializeComponent();
        this.viewModel = vm;
        BindingContext = vm;

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
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

    
   
}