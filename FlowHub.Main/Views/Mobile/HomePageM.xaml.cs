
namespace FlowHub.Main.Views.Mobile;

[QueryProperty(nameof(StartAction), nameof(StartAction))]
public partial class HomePageM : ContentPage
{
    


    int startAction;
    public int StartAction
    {
        get => startAction;
        set
        {
            startAction = value;
            OnPropertyChanged(nameof(StartAction));
            RunAppStartAction();
        }
    }
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
        viewModel.GetUserData();
        if (!_isInitialized)
        {
            await viewModel.DisplayInfo();
            _isInitialized = true;
        }
    }

    void RunAppStartAction()
    {
        if (StartAction is 1)
        {
            MainThread.BeginInvokeOnMainThread(async () => await viewModel.GoToAddExpenditurePage());
        }
    }


}