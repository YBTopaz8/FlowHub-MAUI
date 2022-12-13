using FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;
using FlowHub.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class DetailsOfMonthlyPlannedExpPageM : ContentPage
{
	DetailsOfMonthlyPlannedExpVM viewModel;
	public DetailsOfMonthlyPlannedExpPageM(DetailsOfMonthlyPlannedExpVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoadedCommand.Execute(null);
		ExpList.ItemsSource = null;
		ExpList.ItemsSource = viewModel.TempList;
    }

    private void RightSwipeDelete_Clicked(object sender, EventArgs e)
    {
		if (sender is SwipeItem swipeItem)
		{
			var expItem = (ExpendituresModel) swipeItem.BindingContext;

			viewModel.DeleteExpFromMonthlyPCommand.Execute(expItem);
						
			ExpList.ItemsSource = null;
            ExpList.ItemsSource = viewModel.TempList;
        }
    }
	private void LeftSwipeEdit_Clicked(object sender, EventArgs e)
    {
		if (sender is SwipeItem swipeItem)
		{
			var expItem = (ExpendituresModel) swipeItem.BindingContext;

			viewModel.GoToEditExpInMonthPCommand.Execute(expItem);
        }
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
		PrintProgressBarIndic.IsVisible = true;
		PrintProgressBarIndic.Progress = 0;
		await PrintProgressBarIndic.ProgressTo(1, 1000, Easing.Linear);

		await viewModel.PrintPDFandShareCommand.ExecuteAsync(null);

		PrintProgressBarIndic.IsVisible = false;
    }
}