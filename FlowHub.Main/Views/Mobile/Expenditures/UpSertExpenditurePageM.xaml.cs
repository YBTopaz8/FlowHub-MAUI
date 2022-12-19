using FlowHub.Main.ViewModels.Expenditures;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class UpSertExpenditurePageM : ContentPage
{
    private readonly UpSertExpenditureVM viewModel;
    public UpSertExpenditurePageM(UpSertExpenditureVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
        AddSecondFlowOut.IsVisible= viewModel.ShowAddSecondExpCheckBox;
        AddSecondFlowOut.IsChecked = false;
        AddThirdFlowOut.IsChecked = false;
        viewModel.SecondExp = new Models.ExpendituresModel();
        viewModel.ThirdExp = new Models.ExpendituresModel();
    }

    private void AddSecondFlowOut_CheckChanged(object sender, EventArgs e)
    {
        viewModel.SecondExp = new Models.ExpendituresModel();
        viewModel.ThirdExp = new Models.ExpendituresModel();
        if (!AddSecondFlowOut.IsChecked)
        {
            AddThirdFlowOut.IsChecked = false;
        }
    }
    private void AddThirdFlowOut_CheckChanged(object sender, EventArgs e)
    {

        viewModel.ThirdExp = new Models.ExpendituresModel();
    }

    private void SaveExpBtn_Clicked(object sender, EventArgs e)
    {
        viewModel.UpSertExpenditureCommand.Execute(null);
    }

}