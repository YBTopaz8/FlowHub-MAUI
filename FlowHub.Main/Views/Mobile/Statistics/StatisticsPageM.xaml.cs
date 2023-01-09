using FlowHub.Main.ViewModels.Statistics;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using System.Diagnostics;

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
		pieChart.Series = viewModel.Series;
		BarChart.Series = viewModel.LineSeries;

		OtherBar.Series = viewModel.Series2;
	}

    private void BarChart_ChartPointPointerDown(IChartView chart, ChartPoint point)
    {
		var indexx = point.SecondaryValue;
		int ind = Convert.ToInt32(indexx);
		Debug.WriteLine(point.PrimaryValue);
		Debug.WriteLine(point.SecondaryValue);
		var test = viewModel.listOfExpDec[ind];

		Debug.WriteLine($"{test.Id} \n {test.Reason}");

    }

}