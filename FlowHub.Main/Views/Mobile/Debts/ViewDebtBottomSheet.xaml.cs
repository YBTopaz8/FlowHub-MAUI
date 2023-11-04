using FlowHub.Main.Utilities.BottomSheet;

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ViewDebtBottomSheet : DrawerView
{
    private readonly ManageDebtsVM viewModel;

    public ViewDebtBottomSheet(ManageDebtsVM vm)
    {
        InitializeComponent();

        viewModel = vm;
        BindingContext = viewModel;
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

    private void ShareImageBtn_Clicked(object sender, EventArgs e)
    {


        var imageButton = sender as ImageButton;
        if (imageButton is not null)
        {
            
            titleWithNameText.Text = $"Hey {viewModel.SingleDebtDetails.PersonOrOrganization.Name}";
            titleWithNameText.FontSize = 40;
            amountWithCurrentText.Text = $"{(viewModel.SingleDebtDetails.Currency == "CAD" || viewModel.SingleDebtDetails.Currency == "USD" ? "$" : viewModel.SingleDebtDetails.Currency)}{viewModel.SingleDebtDetails.Amount}";
            reminderText.Text = $"A Kind Reminder That You Owe {(viewModel.SingleDebtDetails.Currency == "CAD" || viewModel.SingleDebtDetails.Currency == "USD" ? "$" : viewModel.SingleDebtDetails.Currency)}{viewModel.SingleDebtDetails.Amount}";
            DeadLineText.Text = $"{viewModel.SingleDebtDetails.DisplayText}";
            DeadLineDateText.Text = $"{viewModel.SingleDebtDetails.Deadline?.ToString("dd-MMM-yyyy")}";
            userNameText.Text = viewModel.ActiveUser.Username;

            _ = SaveToImageAndShare();
            
        }
    }
}