namespace FlowHub.Main.Views.Desktop.Incomes;

public partial class ManageIncomesD : ContentPage
{
    private readonly ManageIncomesVM viewModel;
    public ManageIncomesD(ManageIncomesVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        //this.BindingContext = vm;
        viewModel.PageLoaded();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        this.BindingContext = viewModel;
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        PopUpCloseResult result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric, new List<string>() { "Amount" }, "Enter New Pocket Money"));
        if (result.Result is PopupResult.OK)
        {
            double NewAmount = (double)result.Data;
            await viewModel.ResetUserPocketMoney(NewAmount);
        }
    }
}