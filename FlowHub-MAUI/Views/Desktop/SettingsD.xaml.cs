namespace FlowHub_MAUI.Views.Desktop;

public partial class SettingsD : ContentPage
{
	public SettingsD(HomePageVM ViewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel;
        this.ViewModel = ViewModel;
    }
    public bool ToLogin { get; }
    public HomePageVM ViewModel { get; }
}