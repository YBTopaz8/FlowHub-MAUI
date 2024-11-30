namespace FlowHub_MAUI.Views.Desktop;

public partial class HomeD : ContentPage
{
	public HomeD(HomePageVM viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

    public HomePageVM ViewModel { get; }

    private void Button_Clicked(object sender, EventArgs e)
    {
        ViewModel.AddCommentUI();
    }
}