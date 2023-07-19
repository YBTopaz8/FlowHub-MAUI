namespace FlowHub.Main.Views.Mobile.Incomes;

public partial class ManageIncomesM : ContentPage
{
    private readonly ManageIncomesVM viewModel;
    List<string> FilterResult { get; set; }
    public ManageIncomesM(ManageIncomesVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
    }
    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintIncomesBtn();
            PrintProgressBarIndic.IsVisible = false;
        }
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
