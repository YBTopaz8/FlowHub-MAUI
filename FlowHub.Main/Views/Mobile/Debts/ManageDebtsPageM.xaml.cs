namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ManageDebtsPageM : ContentPage
{
    readonly ManageDebtsVM viewModel;

    public ManageDebtsPageM(ManageDebtsVM vm)
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
}