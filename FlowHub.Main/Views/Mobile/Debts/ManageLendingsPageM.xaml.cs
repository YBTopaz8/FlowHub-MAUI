using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ManageLendingsPageM : UraniumUI.Pages.UraniumContentPage
{
    readonly ManageDebtsVM viewModel;
    public ManageLendingsPageM(ManageDebtsVM vm)
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

    DateTime lastKeyStroke = DateTime.Now;
    private async void DebtsSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        lastKeyStroke = DateTime.Now;
        var thisKeyStroke = lastKeyStroke;
        await Task.Delay(350);
        if (thisKeyStroke == lastKeyStroke)
        {
            SearchBar searchBar = (SearchBar)sender;
            if (searchBar.Text.Length >= 2)
            {
                viewModel.SearchCommandCommand.Execute(searchBar.Text);
                PendingLentExpander.IsExpanded = true;
                CompletedLentExpander.IsExpanded = true;
            }
            else
            {
                viewModel.ApplyChanges();
                PendingLentExpander.IsExpanded = false;
                CompletedLentExpander.IsExpanded = false;
            }
        }
        
    }

    private void PendingLentExpHeader_Tapped(object sender, TappedEventArgs e)
    {
        if (!PendingLentExpander.IsExpanded && !CompletedLentExpander.IsExpanded)
        {
            PendingLentExpander.IsExpanded = true;
            return;
        }
        if (PendingLentExpander.IsExpanded && !CompletedLentExpander.IsExpanded)
        {
            PendingLentExpander.IsExpanded = false;
            return;
        }
        if (!PendingLentExpander.IsExpanded && CompletedLentExpander.IsExpanded)
        {
            PendingLentExpander.IsExpanded = true;
            CompletedLentExpander.IsExpanded = false;
            return;
        }
        PendingLentExpander.IsExpanded = false;
        CompletedLentExpander.IsExpanded = false;
    }

    private void CompletedLentExpHeader_Tapped(object sender, TappedEventArgs e)
    {
        if (!CompletedLentExpander.IsExpanded && !PendingLentExpander.IsExpanded )
        {
            CompletedLentExpander.IsExpanded = true;
            PendingLentExpander.IsExpanded = false;
            return;
        }
        if (CompletedLentExpander.IsExpanded && !PendingLentExpander.IsExpanded)
        {
            CompletedLentExpander.IsExpanded = false;
            return;
        }
        if (!CompletedLentExpander.IsExpanded && PendingLentExpander.IsExpanded)
        {
            CompletedLentExpander.IsExpanded = true;
            PendingLentExpander.IsExpanded = false;
            return;
        }
        PendingLentExpander.IsExpanded = false;
        CompletedLentExpander.IsExpanded = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CompletedLentExpander.IsExpanded = false;
        PendingLentExpander.IsExpanded = false;
    }

}
