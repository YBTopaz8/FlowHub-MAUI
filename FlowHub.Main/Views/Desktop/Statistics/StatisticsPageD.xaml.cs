using FlowHub.Main.ViewModels.Statistics;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Diagnostics;
using LiveChartsCore;

namespace FlowHub.Main.Views.Desktop.Statistics;

public partial class StatisticsPageD : ContentPage
{
    readonly StatisticsPageVM viewModel;
    public StatisticsPageD(StatisticsPageVM vm)
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

        //viewModel.SelectedMonthName = DateTime.Now.ToString("MMMM");

        //  myPieChart.LegendTextPaint = new SolidColorPaint(SKColors.White);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.SaveState();
    }
    int count;

    private void DateTimeExpGraph_ChartPointPointerDown(LiveChartsCore.Kernel.Sketches.IChartView chart, LiveChartsCore.Kernel.ChartPoint point)
    {
        //if (point is not null)
        //{
        //    Debug.WriteLine(new DateTime((long) point.SecondaryValue));
        //}
    }
}