namespace FlowHub.Main.Views.Mobile.Debts;

public partial class SingleDebtDetailsPageM : ContentPage
{
	public SingleDebtDetailsPageM()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync("..", true);
    }
}