namespace FlowHub.Main.Views.Mobile.Debts;

public partial class SingleDebtDetailsPageM : UraniumContentPage
{
    private readonly ManageDebtsVM viewModel;
    private readonly UpSertDebtVM UpSertVM;
    private UpSertDebtBottomSheet UpSertDebtbSheet;
    private UpSertInstallmentBSheet UpSertInstallmentBSheet;
    public SingleDebtDetailsPageM(ManageDebtsVM vm, UpSertDebtVM upSertDebtVm)
	{
		InitializeComponent();
        this.viewModel = vm;
        BindingContext = vm;

        UpSertVM = upSertDebtVm;

        UpSertDebtbSheet = new(upSertDebtVm);
        UpSertDebtbSheet.BindingContext = upSertDebtVm;

        UpSertInstallmentBSheet = new(upSertDebtVm);
        UpSertInstallmentBSheet.BindingContext = upSertDebtVm;
        
        this.Attachments.Add(UpSertDebtbSheet);
        this.Attachments.Add(UpSertInstallmentBSheet);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
        viewModel.RefreshTitleText();
    }

    private void EditFlowHoldBtn_Clicked(object sender, EventArgs e)
    {
        UpSertVM.SingleDebtDetails = viewModel.SingleDebtDetails;
        UpSertVM.PageLoaded();

        UpSertDebtbSheet.IsPresented = true;
    }
    
    private void UpSertInstallmentTapGR_Tapped(object sender, TappedEventArgs e)
    {
        UpSertVM.SingleDebtDetails = viewModel.SingleDebtDetails;
        
        InstallmentPayments selectedInstallment = null;
        if (sender is FlexLayout)
        {
            var se = sender as FlexLayout;
            selectedInstallment = se.BindingContext as InstallmentPayments;
        }
        else
        {
            var se = sender as Label;
            selectedInstallment = se.Parent.BindingContext as InstallmentPayments;
        }
        UpSertVM.SingleInstallmentPayment = selectedInstallment is null ? new() { AmountPaid = 0, DatePaid = DateTime.Now } : selectedInstallment;
        UpSertVM.selectedInstallmentInitialAmount = selectedInstallment is null ? 0 : selectedInstallment.AmountPaid;
        UpSertVM.IsUpSertInstallmentBSheetPresent= true;


    }

    private void AddInstallmentBtn_Clicked(object sender, EventArgs e)
    {
        UpSertVM.SingleDebtDetails = viewModel.SingleDebtDetails;
        UpSertVM.SingleInstallmentPayment = new InstallmentPayments() { AmountPaid =0 , DatePaid = DateTime.Now };
        UpSertVM.IsUpSertInstallmentBSheetPresent = true;
    }
}