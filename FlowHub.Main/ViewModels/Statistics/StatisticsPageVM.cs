using CommunityToolkit.Maui.Core.Extensions;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace FlowHub.Main.ViewModels.Statistics;

[QueryProperty(nameof(GroupedExpenditures), "GroupedExpList")]
public partial class StatisticsPageVM : ObservableObject
{
    private readonly IExpendituresRepository expendituresService;
    public StatisticsPageVM(IExpendituresRepository expRepo)
    {
        expendituresService = expRepo;
    }

    [ObservableProperty]
    ObservableCollection<DateGroup> groupedExpenditures;

    [ObservableProperty]
    ObservableCollection<ExpendituresModel> expendituresForSelectedMonth;
    [ObservableProperty]
    ISeries[] mySeries;

    [ObservableProperty]
    IEnumerable<ISeries> myPieSeries;
    public LabelVisual MyPieCategoriesTitle { get; set; } =
             new LabelVisual
             {
                 Text = "Flows by Category",
#if ANDROID
                 TextSize = 60,
#elif WINDOWS
                 TextSize = 30,
#endif
                 Padding = new LiveChartsCore.Drawing.Padding(10),
                 Paint = new SolidColorPaint(SKColors.DarkSlateBlue)
             };

    [ObservableProperty]
    Axis[] xAxes;

    [ObservableProperty]
    string[] monthNames;
    [ObservableProperty]
    int[] yearNames;
    [ObservableProperty]
    string selectedMonthName;
    [ObservableProperty]
    int selectedMonthValue;
    [ObservableProperty]
    int selectedYearValue;

    [ObservableProperty]
    string selectedCategoryName;

    string[] AllDataAvailableMonths;

    [ObservableProperty]
    string currency;

    [ObservableProperty]
    int totalNumberOfExpenditures;
    [ObservableProperty]
    double totalMonthlyAmount;
    [ObservableProperty]
    double averageDailyAmountInAMonth;
    [ObservableProperty]
    double biggestAmountInAMonth;
    [ObservableProperty]
    double smallestAmountInAMonth;
    [ObservableProperty]
    SolidColorPaint legendTextPaintL = new (SKColors.DarkSlateBlue);
    [ObservableProperty]
    SolidColorPaint legendTextPaintD = new(SKColors.White);

    [ObservableProperty]
    string biggestExpenditureTooltipText;
    [ObservableProperty]
    string smallestExpenditureTooltipText;

    public void PageLoaded()
    {
        if (!IsLoaded)
        {
            if (GroupedExpenditures is null)
            {
                // Update expList
                var expList = expendituresService.OfflineExpendituresList
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.DateSpent).ToList();

                // Update groupedData
                var groupedData = expList.GroupBy(e => e.DateSpent.Date)
                    .Select(g => new DateGroup(g.Key, g.ToList()))
                    .ToList();

                // Update GroupedExpenditures
                GroupedExpenditures = new ObservableCollection<DateGroup>(groupedData);
                OnPropertyChanged(nameof(GroupedExpenditures));
            }
            CalculateMonthNames();
            CalculateYearNames();
            SelectedMonthName = DateTime.Now.ToString("MMMM");
            SelectedYearValue = DateTime.Now.Year;//.ToString();
            Currency = GroupedExpenditures.First().Currency;

            PopulateDataGridWithSelectedMonthData();
            DisplayAllMonthsExpensesChart();
            IsLoaded = true;
        }
    }

    private void CalculateYearNames()
    {
        var s = GroupedExpenditures.Select(g => g.Date.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToArray();
        YearNames = s;
    }

    private void CalculateMonthNames()
    {
        MonthNames = GroupedExpenditures.Select(g => new DateTime(g.Date.Year, g.Date.Month, 1))
                        .Distinct()
                        .Order()
                        .Select(date => date.ToString("MMMM"))
                        .ToArray();
    }

    ObservableCollection<ExpendituresModel> OriginalExpForSelectedMonth = new();

    [RelayCommand]
    void PopulateDataGridWithSelectedMonthData()
    {
        try
        {
            if (SelectedYearValue == 0)
            {
                SelectedYearValue = DateTime.Now.Year;
            }
            SelectedMonthName ??= DateTime.Now.ToString("MMMM");

            DateTime targetDate = new(SelectedYearValue, DateTime.ParseExact(SelectedMonthName, "MMMM", CultureInfo.CurrentCulture).Month, 1);
            SelectedMonthValue = targetDate.Month;

            ExpendituresForSelectedMonth = GroupedExpenditures
            .Where(g => g.Date.Month == targetDate.Month && g.Date.Year == SelectedYearValue)
            .SelectMany(g => g)
            .ToObservableCollection();

            OriginalExpForSelectedMonth = ExpendituresForSelectedMonth;
            TotalNumberOfExpenditures = ExpendituresForSelectedMonth.Count;
            TotalMonthlyAmount = ExpendituresForSelectedMonth.Sum(e => e.AmountSpent);
            AverageDailyAmountInAMonth = TotalMonthlyAmount / DateTime.DaysInMonth(targetDate.Year, targetDate.Month);
            BiggestAmountInAMonth = ExpendituresForSelectedMonth.Max(e => e.AmountSpent);
            SmallestAmountInAMonth = ExpendituresForSelectedMonth.Min(e => e.AmountSpent);

            var expWithBiggestAmount = ExpendituresForSelectedMonth
            .Aggregate((maxExp, nextExp) => nextExp.AmountSpent > maxExp.AmountSpent ? nextExp : maxExp);

            if (expWithBiggestAmount != null)
            {
                BiggestExpenditureTooltipText = $"  {expWithBiggestAmount.Reason} \n{expWithBiggestAmount.DateSpent:d}";
            }
            var expWithSmallestAmount = ExpendituresForSelectedMonth
            .Aggregate((minExp, nextExp) => nextExp.AmountSpent < minExp.AmountSpent ? nextExp : minExp);

            if (expWithSmallestAmount != null)
            {
                SmallestExpenditureTooltipText = $"  {expWithSmallestAmount.Reason} \n{expWithSmallestAmount.DateSpent:d}";
            }
            DisplaySpecificMonthCategoriesPieChart();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        
    }

    private void DisplayAllMonthsExpensesChart()
    {
        var groupedData = GroupedExpenditures
                                .GroupBy(g => new DateTime(g.Date.Year, g.Date.Month, 1))
                                .Select(g => new
                                {
                                    Date = g.Key,
                                    MonthName = g.Key.ToString("MMMM yyyy"),
                                    TotalAmount = g.Sum(x => x.TotalAmount)
                                })
                                .OrderBy(g => g.Date)
                                .ToList();
        var values = new ObservableCollection<ObservableValue>(groupedData
            .Select(g => new ObservableValue(g.TotalAmount)));

        AllDataAvailableMonths = groupedData.Select(g => g.MonthName).ToArray();

        MySeries = new ISeries[]
        {
            new ColumnSeries<ObservableValue>
            {
                Values = values,
                TooltipLabelFormatter = (chartpoint) =>
                $"{AllDataAvailableMonths[chartpoint.Index]} : {chartpoint.PrimaryValue:n2} {Currency}",
            }
        };
        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = AllDataAvailableMonths,
                LabelsRotation = 45,
            }
        };
    }

    private List<PieChartData> listOfPieSeries;
    private void DisplaySpecificMonthCategoriesPieChart()
    {
        try
        {
            listOfPieSeries = GroupedExpenditures
            .SelectMany(g => g)
            .Where(exp => exp.DateSpent.Month == SelectedMonthValue && exp.DateSpent.Year == SelectedYearValue)
            .GroupBy(exp => exp.Category)
            .Select(g => new PieChartData
            {
                Category = g.Key.ToString(),
                TotalCount = g.Count()
            })
            .ToList();

            MyPieSeries = listOfPieSeries.Select(data => new PieSeries<double>
            {
                Values = new double[] { data.TotalCount },
                Name = data.Category,
                TooltipLabelFormatter = p => $"{p.PrimaryValue} {(p.PrimaryValue == 1 ? "Flow" : "Flows")} in {data.Category}",
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                DataLabelsFormatter = _ => $"{data.Category}",
                IsVisibleAtLegend = true
            }).ToArray();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ERROR HERE " + ex.Message);
        }
    }

    int? lastIndex = null;
    [RelayCommand]
    public async Task BarChartPointHover(ChartPoint point)
    {
        if (point is null)
        {
            return;
        }

        int currentIndex = point.Index;
        if (currentIndex == lastIndex)
        {
            return;
        }

        lastIndex = currentIndex;

        var selectedDateLabel = AllDataAvailableMonths[currentIndex];

        DateTime selectedDate;
        var s = DateTime.TryParseExact(selectedDateLabel, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate);

        SelectedMonthValue = selectedDate.Month;
        SelectedYearValue = selectedDate.Year;
        SelectedMonthName = selectedDate.ToString("MMMM");
        SelectedCategoryName = string.Empty;
        await UpdateExpForSelectedMonthCollection();
    }

    private async Task UpdateExpForSelectedMonthCollection()
    {
        await Task.Delay(50);
        ExpendituresForSelectedMonth = GroupedExpenditures
            .Where(g => g.Date.Month == SelectedMonthValue && g.Date.Year == SelectedYearValue)
            .SelectMany(g => g)
            .ToObservableCollection();
    }

    string lastPieCategory;
    [RelayCommand]
    public async Task PieChartClick(ChartPoint point)
    {
        await Task.Delay(50);
        if (point is null)
        {
            return;
        }

        string currentCategory = point.AsDataLabel;

        if (currentCategory == lastPieCategory)
        {
            ExpendituresForSelectedMonth = OriginalExpForSelectedMonth;
            return;
        }

        lastPieCategory = currentCategory;

        SelectedCategoryName = currentCategory;

        await UpdateExpForSelectedMonthToShowOnlyCategories();
    }

    private async Task UpdateExpForSelectedMonthToShowOnlyCategories()
    {
        await Task.Delay(50);
        ExpendituresForSelectedMonth = OriginalExpForSelectedMonth.Where(e => e.Category.ToString() == SelectedCategoryName)
            .ToObservableCollection();
    }

    //section for state management
    public bool IsLoaded;

    public static Dictionary<string, object> ViewModelStateCache = new Dictionary<string, object>();

    public void SaveState()
    {
        ViewModelStateCache["SelectedMonthName"] = SelectedMonthName;
        ViewModelStateCache["SelectedYearValue"] = SelectedYearValue;
        ViewModelStateCache["MonthNames"] = MonthNames;
        ViewModelStateCache["YearNames"] = YearNames;
        ViewModelStateCache["Currency"] = Currency;
        //... add all properties that you want to persist
    }

    public void LoadState()
    {
        if (ViewModelStateCache.ContainsKey("SelectedMonthName"))
        {
            SelectedMonthName = (string)ViewModelStateCache["SelectedMonthName"];
        }
        if (ViewModelStateCache.ContainsKey("SelectedYearValue"))
        {
            SelectedYearValue = (int)ViewModelStateCache["SelectedYearValue"];
        }
        if (ViewModelStateCache.ContainsKey("MonthNames"))
        {
            MonthNames = (string[])ViewModelStateCache["MonthNames"];
        }
        if (ViewModelStateCache.ContainsKey("YearNames"))
        {
            YearNames = (int[])ViewModelStateCache["YearNames"];
        }
        if (ViewModelStateCache.ContainsKey("Currency"))
        {
            Currency = (string)ViewModelStateCache["Currency"];
        }
    }

    //old code below
    public List<ExpendituresModel> listOfExp { get; set; }
    public ExpendituresModel[] listOfExpDec { get; set; }

    public List<ISeries> Series { get; set; }
    public List<ISeries> Series2 { get; set; }

    public Axis[] YAxes { get; set; } =
    {
        new Axis { MinLimit = 0, MaxLimit = 50 }
    };

    public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint
    {
        Color = new SKColor(240, 240, 240)
    };
    //public SolidColorPaint LegendBGPaint { get; set; } = new SolidColorPaint(new SkiaSharp.SKColor(240, 240, 240));

    public ObservableCollection<ISeries> LineSeries { get; set; }

    [RelayCommand]
    //public void PageLoaded()
    //{
    //	Series2 = new ()
    //	{
    //		new ColumnSeries<double>
    //		{
    //			IsHoverable = false, // disables the series from the tooltips 
    //			Values = new double[] { 44, 20, 49, 10, 12, 38, 7 },
    //			Stroke = null,
    //			Fill = new SolidColorPaint(new SKColor(30, 30, 30, 30)),

    //			IgnoresBarPosition = false
    //		}
    //	};

    //	listOfExp = new();

    //	listOfExp = expendituresService.OfflineExpendituresList;

    //	//Don't forget to add year == current year in condition
    //	listOfExpDec = new ExpendituresModel[listOfExp.Count];
    //	listOfExpDec = listOfExp.Where(x => x.DateSpent.Month == 12).ToArray();

    //	/*
    //	TestClass first = new("first", 1) ;

    //	TestClass sec = new("Second", 5);
    //	TestClass th = new("Third", 17);
    //	TestClass frth = new("Fourth", 9);
    //	TestClass ffth = new("Fifth", 13);
    //	List<TestClass> testClasses = new()
    //	{
    //		first,
    //		sec,
    //		th,
    //		frth,
    //		ffth
    //	};
    //	var listOfLineSeries =new List<LineSeries<double>>();
    //       */
    //	var listOfPieSeries = new List<PieSeries<double>>();

    //	foreach (var item in listOfExpDec)
    //	{
    //		listOfPieSeries.Add(new PieSeries<double>{
    //			Name = item.Reason,
    //			Values = new double[] { item.AmountSpent },
    //			TooltipLabelFormatter =
    //				(ChartPoint) => $"{ChartPoint.Context.Series.Name }: {ChartPoint.PrimaryValue:n3} {item.Currency}",
    //		});
    //	}

    //	Series = new List<ISeries>();
    //	LineSeries = new ObservableCollection<ISeries>();

    //	Series.AddRange(listOfPieSeries);

    //       LineSeries<ExpendituresModel> LinesSeriesToPlot = new()
    //	{
    //		Name = $"Graph of {listOfExpDec.Length} Flow Outs For December 2022",
    //		TooltipLabelFormatter = (point) => $"{point.Model.Reason} : {point.Model.AmountSpent:n3} {point.Model.Currency}\n" +
    //		$"{point.Model.DateSpent:dd-MMM-yy}",
    //		Values = listOfExpDec,
    //		Mapping =(testt, point) =>
    //		{
    //			point.PrimaryValue = (double)testt.AmountSpent;
    //			point.SecondaryValue = point.Context.Entity.EntityIndex;
    //		},
    //		Stroke = new SolidColorPaint(SKColors.DarkSlateBlue) { StrokeThickness = 4 },

    //	//	Name = "Plot",
    //	//	DataLabelsSize = 30,

    //	//	DataLabelsPaint = new SolidColorPaint(SKColors.White),
    //	//	DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
    //	//	Fill = null,
    //	//	DataLabelsFormatter = (Series) => Series.SecondaryValue.ToString(),
    //	//	Values = dataValues,
    // //          GeometrySize = 0,
    // //          LineSmoothness = 0
    //       };

    //       LineSeries.Add(LinesSeriesToPlot);
    //	//LineSeries = new List<ISeries>()
    //	//{
    //	//	new LineSeries<double>
    //	//	{				
    //	//		DataLabelsFormatter = (point) => point.PrimaryValue.ToString(),
    //	//		Values = new double[] { 5, 0, 5, 0, 5, 0 },
    //	//		Fill = null,
    //	//          },

    //	//          new LineSeries<double>
    //	//	{
    //	//		Values = new double[] { 7, 2, 7, 2, 7, 2 },
    //	//		Fill = null,
    //	//		GeometrySize = 0,
    //	//		LineSmoothness = 1
    //	//	}

    //	//};

    //}

    void GetNov()
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
                point.SecondaryValue = point.Index;
            },
        };

        LineSeries<ExpendituresModel> LinesSeriesToPlot = new()
        {
            Name = $"Graph of {listOfExpDec.Length} Flow Outs For November 2022",
            TooltipLabelFormatter = (point) =>
            $"{point.Model.Reason} : {point.Model.AmountSpent:n2} {point.Model.Currency}\n" +
            $"{point.Model.DateSpent:dd-MMM-yy}",
            Values = listOfExpDec,
            Mapping = (testt, point) =>
            {
                point.PrimaryValue = (double)testt.AmountSpent;
                point.SecondaryValue = point.Index;
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
            val = Val;
        }
    }

    public class PieChartData
    {
        public double TotalCount { get; set; }
        public string Category { get; set; }
    }
}
