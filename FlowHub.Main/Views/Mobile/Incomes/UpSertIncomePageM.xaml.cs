using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Main.ViewModels.Incomes;

namespace FlowHub.Main.Views.Mobile.Incomes;

public partial class UpSertIncomePageM : ContentPage
{
	private readonly UpSertIncomeVM viewModel;
	public UpSertIncomePageM(UpSertIncomeVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoadedCommand.Execute(null);
    }
}