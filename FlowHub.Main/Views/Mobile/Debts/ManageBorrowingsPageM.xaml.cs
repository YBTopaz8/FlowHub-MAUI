
namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ManageBorrowingsPageM : UraniumUI.Pages.UraniumContentPage
{
    readonly ManageDebtsVM viewModel;
    public ManageBorrowingsPageM(ManageDebtsVM vm)
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

    private void DebtsSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {

        SearchBar searchBar = (SearchBar)sender;
        //DebtsSearchBar.ItemsSource = DataService.GetSearchResults(searchBar.Text);
        viewModel.SearchCommandCommand.Execute(searchBar.Text);
    }
}
