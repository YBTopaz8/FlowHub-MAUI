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
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.DisplayInfo();
    }
}