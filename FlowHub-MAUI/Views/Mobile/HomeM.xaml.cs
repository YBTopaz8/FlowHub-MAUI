namespace FlowHub_MAUI.Views.Mobile;

public partial class HomeM : ContentPage
{
	public HomeM(HomePageVM viewModel)
	{
		InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

    public HomePageVM ViewModel { get; }
}