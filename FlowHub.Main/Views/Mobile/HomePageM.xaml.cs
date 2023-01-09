using FlowHub.Main.ViewModels;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile;

public partial class HomePageM : ContentPage
{
    public readonly HomePageVM viewModel;
    public HomePageM(HomePageVM vm)
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