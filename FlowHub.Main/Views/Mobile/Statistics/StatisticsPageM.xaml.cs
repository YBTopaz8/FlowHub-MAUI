using FlowHub.Main.ViewModels.Statistics;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using System.Diagnostics;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;


namespace FlowHub.Main.Views.Mobile.Statistics;

public partial class StatisticsPageM
{
	private readonly StatisticsPageVM viewModel;
	public StatisticsPageM(StatisticsPageVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        viewModel.LoadState();

        if (!viewModel.IsLoaded)
        {
            await viewModel.PageLoaded();
            YearPicker.SelectedItem = DateTime.Now.Year.ToString();
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.SaveState();
    }
    protected override bool OnBackButtonPressed()
    {
        Debug.WriteLine("Back button pressed");
        return base.OnBackButtonPressed();
        
    }
    private void BarChart_ChartPointPointerDown(IChartView chart, ChartPoint point)
    {
        
    }
}