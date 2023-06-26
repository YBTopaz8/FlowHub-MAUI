using FlowHub.Main.ViewModels.Expenditures;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class ManageExpendituresPageD : ContentPage
{
	readonly ManageExpendituresVM viewModel;
	public ManageExpendituresPageD(ManageExpendituresVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageloadedAsyncCommand.Execute(null);
    }
}