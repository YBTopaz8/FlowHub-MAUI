using FlowHub.Main.Utilities.BottomSheet;

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ViewDebtBottomSheet : DrawerView
{
    private readonly ManageDebtsVM viewModel;

    public ViewDebtBottomSheet(ManageDebtsVM vm)
    {
        InitializeComponent();

        viewModel = vm;
        BindingContext = viewModel;
    }
}