using FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class ManageMonthlyPlannedExpendituresPageM : ContentPage
{
	private ManageMonthlyMonthlyPlannedExpendituresVM viewModel;
	public ManageMonthlyPlannedExpendituresPageM(ManageMonthlyMonthlyPlannedExpendituresVM vm)
	{
		InitializeComponent();
		viewModel= vm;
		this.BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);

    }

}