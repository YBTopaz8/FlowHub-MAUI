namespace FlowHub.Main.Views.Desktop.Debts;

public partial class ManageDebtsPageD : ContentPage
{
    readonly ManageDebtsVM viewModel;
    public ManageDebtsPageD(ManageDebtsVM vm)
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