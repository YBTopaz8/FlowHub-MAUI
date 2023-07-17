namespace FlowHub.Main.Views.Mobile;

public partial class HomePageM : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageM(HomePageVM vm)
	{
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }

    bool _isInitialized;
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if(!_isInitialized)
        {
            await viewModel.DisplayInfo();
            _isInitialized = true;
        }
    }
}