using FlowHub.Main.ViewModels.Statistics;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using System.Diagnostics;
using FlowHub.Models;

namespace FlowHub.Main.Views.Mobile.Statistics;

public partial class SingleMonthStatsPageM : ContentPage
{
	private readonly SingleMonthStatsPageVM viewModel;
	ExpendituresModel SpecificExp { get; set; } = new();
	public SingleMonthStatsPageM(SingleMonthStatsPageVM vm)
	{
		InitializeComponent();
		viewModel= vm;
		this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoadedCommand.Execute(null);
		pieChart.Series = viewModel.PieSeries;
        SelectedTitle.Text = "Biggest Flow Out Details";

    }
	ChartPoint obj = null;
    private void pieChart_ChartPointPointerDown(IChartView chart, ChartPoint point)
    {
		if (point is not null)
		{
			obj=point;
			SelectedTitle.Text = "Selected Flow Out Details";
			var SelectedExpIndex = Convert.ToInt32(point.TertiaryValue);
			viewModel.ChangeSelectedExp(SelectedExpIndex);
		}

    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

		Debug.WriteLine($"Double Tapped {obj.PrimaryValue}");
    }
}