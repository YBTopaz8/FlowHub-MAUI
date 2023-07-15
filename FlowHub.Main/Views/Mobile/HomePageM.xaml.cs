namespace FlowHub.Main.Views.Mobile;

public partial class HomePageM : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageM(HomePageVM vm)
	{
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }

    bool _isInitialized;
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if(!_isInitialized)
        {
            await viewModel.DisplayInfo();
            await viewModel.incomeRepo.SynchronizeIncomesAsync();
            _isInitialized = true;
        }

    }
}