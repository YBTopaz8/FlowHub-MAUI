using FlowHub.Main.ViewModels;

namespace FlowHub.Main.Views.Desktop;

public partial class HomePageD : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageD(HomePageVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.DisplayInfoCommand.Execute(null);
    }
}