namespace FlowHub.Main.Views.Mobile.Debts;

public partial class DebtsOverviewPageM : UraniumUI.Pages.UraniumContentPage
{
    readonly ManageDebtsVM viewModel;

    public DebtsOverviewPageM(ManageDebtsVM vm)
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


    private async void LentBrdr_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ManageLendingsPageM), true);
    }

    private async void BorrowBrdr_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ManageBorrowingsPageM), true);
    }


    /*
    private async void SearchBarViewToggler_Clicked(object sender, EventArgs e)
    {
        if (SearchBarView.IsVisible)
        {
            // Hide SearchBarView and move RestOfPage back to original position
            var searchBarFadeOutTask = SearchBarView.FadeTo(0, 400);
            var restOfPageMoveUpTask = RestOfPage.TranslateTo(0, 0, 400);

            await Task.WhenAll(searchBarFadeOutTask, restOfPageMoveUpTask);

            SearchBarView.IsVisible = false;
            RestOfPage.TranslationY = 0; // Reset position
        }
        else
        {
            SearchBarView.IsVisible = true;

            // Initialize position and opacity for the animation
            SearchBarView.Opacity = 0;
            RestOfPage.TranslationY = 0; // Start from original position

            // Show SearchBarView and move RestOfPage down
            var searchBarFadeInTask = SearchBarView.FadeTo(1, 400);
            var restOfPageMoveDownTask = RestOfPage.TranslateTo(0, SearchBarView.Height-200, 400);

            await Task.WhenAll(searchBarFadeInTask, restOfPageMoveDownTask);

            RestOfPage.TranslationY = SearchBarView.Height; // Ensure it stays at the intended position
        }
    
    }*/


}