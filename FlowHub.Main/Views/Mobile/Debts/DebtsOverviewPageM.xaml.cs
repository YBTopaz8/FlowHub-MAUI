namespace FlowHub.Main.Views.Mobile.Debts;

public partial class DebtsOverviewPageM : UraniumContentPage
{
    readonly ManageDebtsVM viewModel;
    readonly UpSertDebtVM UpSertVM;
    private UpSertDebtBottomSheet UpSertDebtbSheet;
    public DebtsOverviewPageM(ManageDebtsVM vm, UpSertDebtVM upSertDebt)
    {
        InitializeComponent();

        viewModel = vm;
        UpSertVM = upSertDebt;        

        UpSertDebtbSheet = new(upSertDebt);
        this.Attachments.Add(UpSertDebtbSheet);

    }
    protected override void OnAppearing()
    {
        if (UpSertDebtbSheet.IsPresented)
        {
            UpSertDebtbSheet.IsPresented = false;
        }
        base.OnAppearing();
        viewModel.PageLoaded();
        UpSertDebtbSheet.IsPresented = false;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        this.BindingContext = viewModel;
    }
    private async void LentBrdr_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ManageLendingsPageM), true);
    }

    private async void BorrowBrdr_Tapped(object sender, TappedEventArgs e)
    {
        if (!UpSertDebtbSheet.IsPresented)
        {
            await Shell.Current.GoToAsync(nameof(ManageBorrowingsPageM), true);
        }
    }

   
    private void AddNewFlowHoldBtn_Clicked(object sender, EventArgs e)
    {
        UpSertVM.SingleDebtDetails = new DebtModel()
        {
            Amount = 1,
            PersonOrOrganization = new PersonOrOrganizationModel(),
            Currency = viewModel.UserCurrency,
        };

        UpSertVM.PageLoaded();
        UpSertDebtbSheet.IsPresented = true;
        
    }


}