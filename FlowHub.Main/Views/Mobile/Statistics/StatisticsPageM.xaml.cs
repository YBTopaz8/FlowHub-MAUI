using FlowHub.Main.ViewModels.Statistics;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

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
    }

   
}