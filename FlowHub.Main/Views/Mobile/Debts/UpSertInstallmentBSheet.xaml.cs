

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class UpSertInstallmentBSheet : BottomSheetView
{
    readonly UpSertDebtVM viewModel;
    public bool ShowDeleteBtn { get; set; }
    public UpSertInstallmentBSheet(UpSertDebtVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnOpened()
    {
        base.OnOpened();
        DeleteInstallmentBtn.IsVisible = ShowDeleteBtn;
    }
    private void AmountPaid_Focused(object sender, FocusEventArgs e)
    {
        if (AmountPaid.Text == "1")
        {
            AmountPaid.Text = "";
        }
    }
}