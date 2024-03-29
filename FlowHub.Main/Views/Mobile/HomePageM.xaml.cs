

namespace FlowHub.Main.Views.Mobile;

[QueryProperty(nameof(StartAction), nameof(StartAction))]
public partial class HomePageM : UraniumContentPage
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
    public readonly UpSertExpenditureVM UpSertExpVM;
    private UpSertExpenditureBottomSheet UpSertExpbSheet;
    public HomePageM(HomePageVM vm, UpSertExpenditureVM UpSertExpVm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
        UpSertExpVM = UpSertExpVm;

        UpSertExpbSheet = new(UpSertExpVm);
        Attachments.Add(UpSertExpbSheet);
    }
    
    protected override async void OnAppearing()
    {
        if (UpSertExpbSheet.IsPresented)
        {
            UpSertExpbSheet.IsPresented = false;
        }
        base.OnAppearing();     
        viewModel.GetUserData();
        if (!viewModel._isInitialized)
        {
            await viewModel.DisplayInfo();
            viewModel._isInitialized = true;
        }
    }

    void RunAppStartAction()
    {
        if (StartAction is 1)
        {
            MainThread.BeginInvokeOnMainThread(async () => await viewModel.GoToAddExpenditurePage());
        }
    }

    private void AddExpBtn_Clicked(object sender, EventArgs e)
    {
        UpSertExpVM.SingleExpenditureDetails = new()
        {
            DateSpent = DateTime.Now,
        };

        UpSertExpVM.PageLoaded();
        UpSertExpbSheet.IsPresented = true;
        Shell.Current.IsEnabled = false;
        
    }

}