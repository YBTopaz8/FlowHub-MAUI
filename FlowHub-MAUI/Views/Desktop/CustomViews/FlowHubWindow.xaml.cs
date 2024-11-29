namespace FlowHub_MAUI.Views.Desktop.CustomViews;

public partial class FlowHubWindow : Window
{
	public FlowHubWindow(Lazy<HomePageVM> viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel.Value;
        BindingContext = viewModel.Value;
    }

    public HomePageVM ViewModel { get; }

    protected override void OnCreated()
    {
        base.OnCreated();
        this.MinimumHeight = 950;
        this.MinimumWidth = 1200;
        this.Height = 950;
        this.Width = 1200;

#if DEBUG
        FlowHubTitleBar.Subtitle = "v0.0.1-debug";
#endif

#if RELEASE
        FlowHubTitleBar.Subtitle = "v0.0.1-release";
#endif
    }
}