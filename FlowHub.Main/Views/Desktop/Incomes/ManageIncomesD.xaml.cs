namespace FlowHub.Main.Views.Desktop.Incomes;

public partial class ManageIncomesD : ContentPage
{
	private readonly ManageIncomesVM viewModel;
	public ManageIncomesD(ManageIncomesVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
        viewModel.PageLoaded();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}