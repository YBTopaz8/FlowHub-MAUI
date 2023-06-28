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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);

        YearPicker.SelectedItem = DateTime.Now.Year.ToString();
        viewModel.SelectedMonthName = DateTime.Now.ToString("MMMM");

        myPieChart.LegendTextPaint = new SolidColorPaint(SKColors.White);
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