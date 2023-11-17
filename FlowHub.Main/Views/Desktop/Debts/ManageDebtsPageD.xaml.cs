using UraniumUI.Pages;

namespace FlowHub.Main.Views.Desktop.Debts;

public partial class ManageDebtsPageD : UraniumContentPage
{
    readonly ManageDebtsVM viewModel;
    readonly UpSertDebtVM UpSertVM;
    public ManageDebtsPageD(ManageDebtsVM vm, UpSertDebtVM upSertDebt)
    {
        InitializeComponent();
        viewModel = vm;
        UpSertVM = upSertDebt;
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
    }

    DateTime lastKeyStroke = DateTime.Now;
    private async void DebtsSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        lastKeyStroke = DateTime.Now;
        var thisKeyStroke = lastKeyStroke;
        await Task.Delay(350);
        if (thisKeyStroke == lastKeyStroke)
        {
            SearchBar searchBar = (SearchBar)sender;
            if (searchBar.Text.Length >= 2)
            {
                viewModel.SearchBarCommand.Execute(searchBar.Text);
            }
            else
            {
                viewModel.ApplyChanges();
            }
        }

    }

    private void ShareImageBtn_Clicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        if (imageButton is not null)
        {
            var debtModel = imageButton.BindingContext as DebtModel;
            if (debtModel is not null)
            {
                titleWithNameText.Text = $"Hey {debtModel.PersonOrOrganization.Name}";
                titleWithNameText.FontSize = 40;
                amountWithCurrentText.Text = $"{(debtModel.Currency == "CAD" || debtModel.Currency == "USD" ? "$" : debtModel.Currency)}{debtModel.Amount}";
                reminderText.Text = $"A Kind Reminder That You Owe {(debtModel.Currency == "CAD" || debtModel.Currency == "USD" ? "$" : debtModel.Currency)}{debtModel.Amount}";
                DeadLineText.Text = $"{debtModel.DisplayText}";
                DeadLineDateText.Text = $"{debtModel.Deadline?.ToString("dd-MMM-yyyy")}";
                userNameText.Text = viewModel.ActiveUser.Username;

                _ = SaveToImageAndShare();
            }
        }
    }

    private async Task<MemoryStream> SaveToImageAndShare()
    {
        DebtReminderCard.IsVisible = true;
        await Task.Delay(1500);
        var image = await DebtReminderCard.CaptureAsync();
        var memoryStream = new MemoryStream();
        memoryStream.Position = 0;
        await image.CopyToAsync(memoryStream);

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FlowHub\FlowHoldsShares");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fp = $@"{path}\imag2e.png";
        File.WriteAllBytes(fp, memoryStream.ToArray());

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share Flow Hold Reminder",
            File = new ShareFile(fp)
        });
        DebtReminderCard.IsVisible = false;
        return memoryStream;
    }

   
}