namespace FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class DetailsOfMonthlyPlannedExpPageM : ContentPage
{
    DetailsOfMonthlyPlannedExpVM viewModel;
    public DetailsOfMonthlyPlannedExpPageM(DetailsOfMonthlyPlannedExpVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
        ExpList.ItemsSource = null;
        ExpList.ItemsSource = viewModel.TempList;
    }

    private async void RightSwipeDelete_Clicked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
        {
            var expItem = (ExpendituresModel)swipeItem.BindingContext;

            await viewModel.DeleteExpFromMonthlyP(expItem);

            ExpList.ItemsSource = null;
            ExpList.ItemsSource = viewModel.TempList;
        }
    }
    private async void LeftSwipeEdit_Clicked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem)
        {
            var expItem = (ExpendituresModel)swipeItem.BindingContext;
            //redo this as a command
            await viewModel.GoToEditExpInMonthP(expItem);
        }
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        PrintProgressBarIndic.IsVisible = true;
        PrintProgressBarIndic.Progress = 0;
        await PrintProgressBarIndic.ProgressTo(1, 1000, Easing.Linear);

        await viewModel.PrintPDFandShare();

        PrintProgressBarIndic.IsVisible = false;
    }
}