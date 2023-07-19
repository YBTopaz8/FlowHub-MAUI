namespace FlowHub.Main.Views.Desktop;

public partial class HomePageD : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageD(HomePageVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }
    bool _isInitialized;
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await viewModel.DisplayInfo();
            _isInitialized = true;
        }
    }
}