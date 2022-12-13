using FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

namespace FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class UpSertMonthlyPlannedExpPageM : ContentPage
{
	private readonly UpSertMonthlyPlannedExpVM viewModel;
	public UpSertMonthlyPlannedExpPageM(UpSertMonthlyPlannedExpVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
		Comments.Text = "";
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoadedCommand.Execute(null);
		CommentCheck.IsChecked = viewModel.HasComment;
    }

    private void CommentCheck_CheckChanged(object sender, EventArgs e)
    {
		if (!CommentCheck.IsChecked)
		{
			viewModel.SingleExpenditureDetails.Comment = "None";
		}
    }
}