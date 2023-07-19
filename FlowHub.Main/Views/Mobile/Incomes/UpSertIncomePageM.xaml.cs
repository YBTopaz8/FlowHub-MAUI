namespace FlowHub.Main.Views.Mobile.Incomes;

public partial class UpSertIncomePageM : ContentPage
{
    private readonly UpSertIncomeVM viewModel;
    public UpSertIncomePageM(UpSertIncomeVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
    }

    private void SaveIncBtn_Clicked(object sender, EventArgs e)
    {
        viewModel.UpSertIncomeCommand.Execute(null);
    }
}