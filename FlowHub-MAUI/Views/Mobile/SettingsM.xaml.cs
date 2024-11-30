namespace FlowHub_MAUI.Views.Mobile;

public partial class SettingsM : ContentPage
{
	public SettingsM(HomePageVM ViewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel;
        this.ViewModel = ViewModel;
    }
    public bool ToLogin { get; }
    public HomePageVM ViewModel { get; }
}