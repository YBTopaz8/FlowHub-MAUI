using FlowHub.Main.ViewModels.Incomes;

namespace FlowHub.Main.Views.Desktop.Incomes;

public partial class ManageIncomesD : ContentPage
{
	private readonly ManageIncomesVM viewModel;
	public ManageIncomesD(ManageIncomesVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoaded();
    }
}