using FlowHub.Main.ViewModels.Expenditures;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class UpSertExpenditurePageM : ContentPage
{
    private UpSertExpenditureVM viewModel;
    public UpSertExpenditurePageM(UpSertExpenditureVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
   //     viewModel.PageLoadedCommand.Execute(null);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
    }

}