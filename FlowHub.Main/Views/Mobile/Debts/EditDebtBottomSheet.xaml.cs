using FlowHub.Main.Utilities.BottomSheet;

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class EditDebtBottomSheet : DrawerView
{
    
    private readonly ManageDebtsVM viewModel;

    public EditDebtBottomSheet(ManageDebtsVM vm)
	{
		InitializeComponent();
        
        viewModel = vm;
        BindingContext = viewModel;
        
    }

}