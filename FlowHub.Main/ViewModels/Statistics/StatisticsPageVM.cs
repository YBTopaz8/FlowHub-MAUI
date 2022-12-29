using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace FlowHub.Main.ViewModels.Statistics;

public partial class StatisticsPageVM : ObservableObject
{
	private readonly IExpendituresRepository expendituresService;
	public StatisticsPageVM(IExpendituresRepository expRepo)
	{
		expendituresService = expRepo;
	}

	
    public List<ExpendituresModel> listOfExp { get; set; }
    public ExpendituresModel[] listOfExpDec { get; set; }

    public List<ISeries> Series { get; set; }
	public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint
	{
		Color = new SKColor(240,240,240)
	};
	//public SolidColorPaint LegendBGPaint { get; set; } = new SolidColorPaint(new SkiaSharp.SKColor(240, 240, 240));


	public ObservableCollection<ISeries> LineSeries { get; set; }



	[RelayCommand]
	public void PageLoaded()
	{
		listOfExp = new();

		listOfExp = expendituresService.OfflineExpendituresList;

		//Don't forget to add year == current year in condition
		listOfExpDec = new ExpendituresModel[listOfExp.Count];
		listOfExpDec = listOfExp.Where(x => x.DateSpent.Month == 12).ToArray();

		/*
		TestClass first = new("first", 1) ;

		TestClass sec = new("Second", 5);
		TestClass th = new("Third", 17);
		TestClass frth = new("Fourth", 9);
		TestClass ffth = new("Fifth", 13);
		List<TestClass> testClasses = new()
		{
			first,
			sec,
			th,
			frth,
			ffth
		};
		var listOfLineSeries =new List<LineSeries<double>>();
        */
		var listOfPieSeries = new List<PieSeries<double>>();

		foreach (var item in listOfExpDec)
		{
			listOfPieSeries.Add(new PieSeries<double>{ 
				
				Name = item.Reason,
				Values = new double[] { item.AmountSpent },
				TooltipLabelFormatter =
					(ChartPoint) => $"{ChartPoint.Context.Series.Name }: {ChartPoint.PrimaryValue:n3} {item.Currency}",
			});			
		}
		
		Series = new List<ISeries>();
		LineSeries = new ObservableCollection<ISeries>();
		
		Series.AddRange(listOfPieSeries);

        LineSeries<ExpendituresModel> LinesSeriesToPlot = new()
		{
			Name = $"Graph of {listOfExpDec.Length} Flow Outs For December 2022",
			TooltipLabelFormatter = (point) => $"{point.Model.Reason} : {point.Model.AmountSpent:n3} {point.Model.Currency}\n" +
			$"{point.Model.DateSpent:dd-MMM-yy}",
			Values = listOfExpDec,
			Mapping =(testt, point) =>
			{
				point.PrimaryValue = (double)testt.AmountSpent;
				point.SecondaryValue = point.Context.Entity.EntityIndex; 
			},
			Stroke = new SolidColorPaint(SKColors.DarkSlateBlue) { StrokeThickness = 4 },
			
			
		//	Name = "Plot",
		//	DataLabelsSize = 30,
			
		//	DataLabelsPaint = new SolidColorPaint(SKColors.White),
		//	DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
		//	Fill = null,
		//	DataLabelsFormatter = (Series) => Series.SecondaryValue.ToString(),
		//	Values = dataValues,
  //          GeometrySize = 0,
  //          LineSmoothness = 0
        };

        LineSeries.Add(LinesSeriesToPlot);
		//LineSeries = new List<ISeries>()
		//{
		//	new LineSeries<double>
		//	{				
		//		DataLabelsFormatter = (point) => point.PrimaryValue.ToString(),
		//		Values = new double[] { 5, 0, 5, 0, 5, 0 },
		//		Fill = null,
		//          },

		//          new LineSeries<double>
		//	{
		//		Values = new double[] { 7, 2, 7, 2, 7, 2 },
		//		Fill = null,
		//		GeometrySize = 0,
		//		LineSmoothness = 1
		//	}

		//};

	}

	[RelayCommand]
	void GetNov ()
	{

        var listOfExp = expendituresService.OfflineExpendituresList;

        var listOfExpDec = listOfExp.Where(x => x.DateSpent.Month == 11).ToArray();


		PieSeries<ExpendituresModel> PieSeriesToPlot = new PieSeries<ExpendituresModel>()
		{
			Name = $"Graph of {listOfExpDec.Length} Flow Outs For November 2022",
			Values = listOfExpDec,
			Mapping = (testt, point) =>
			{
				point.PrimaryValue = (double)testt.AmountSpent;
				point.SecondaryValue = point.Context.Entity.EntityIndex;

			},

		};

        LineSeries<ExpendituresModel> LinesSeriesToPlot = new()
        {
            Name = $"Graph of {listOfExpDec.Length} Flow Outs For November 2022",
            TooltipLabelFormatter = (point) => $"{point.Model.Reason} : {point.Model.AmountSpent:n3} {point.Model.Currency}\n" +
			$"{point.Model.DateSpent:dd-MMM-yy}",
            Values = listOfExpDec,
            Mapping = (testt,  point) =>
            {
                point.PrimaryValue = (double)testt.AmountSpent;
                point.SecondaryValue = point.Context.Entity.EntityIndex;
				
            },
            Stroke = new SolidColorPaint(SKColors.DarkSlateBlue) { StrokeThickness = 4 },


            //	Name = "Plot",
            //	DataLabelsSize = 30,

            //	DataLabelsPaint = new SolidColorPaint(SKColors.White),
            //	DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
            //	Fill = null,
            //	DataLabelsFormatter = (Series) => Series.SecondaryValue.ToString(),
            //	Values = dataValues,
            //          GeometrySize = 0,
            //          LineSmoothness = 0
        };
		LineSeries.Clear();
        LineSeries.Add(LinesSeriesToPlot);
    }

   public class TestClass
	{
		public string Name { get; set; }
		public double val { get; set; }

		public TestClass(string name, double Val)
		{
			Name = name;
			val=Val;
		}
	}
}

