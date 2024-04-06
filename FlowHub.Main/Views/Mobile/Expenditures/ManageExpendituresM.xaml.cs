using Microsoft.Maui.Controls.Platform;
using System.Runtime.CompilerServices;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class ManageExpendituresM : UraniumContentPage
{
    private readonly ManageExpendituresVM viewModel;
    private readonly UpSertExpenditureVM upSertExpVM;
    private UpSertExpenditureBottomSheet UpSertExpbSheet;
    public ManageExpendituresM(ManageExpendituresVM vm, UpSertExpenditureVM upSertExpVM)
    {
        InitializeComponent();
        viewModel = vm;
        this.upSertExpVM = upSertExpVM;
        BindingContext = vm;

        UpSertExpbSheet = new(upSertExpVM);
        Attachments.Add(UpSertExpbSheet);
        
    }

    protected override async void OnAppearing()
    {
        if (UpSertExpbSheet.IsPresented)
        {
            UpSertExpbSheet.IsPresented = false;
        }
        base.OnAppearing();
        
        await viewModel.PageloadedAsync();
        
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.ExpendituresList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintExpendituresBtn();
            PrintProgressBarIndic.IsVisible = false;
        }
    }

    private void AddExpBtn_Clicked(object sender, EventArgs e)
    {
        upSertExpVM.SingleExpenditureDetails = new()
        {
            DateSpent = DateTime.Now,
        };
        upSertExpVM.PageLoaded();
        UpSertExpbSheet.IsPresented = true;
    }

    private void EditExpIcon_Clicked(object sender, EventArgs e)
    {
        var imageBtn = sender as SwipeItem;
        var singleExp = (ExpendituresModel)imageBtn.BindingContext;

        upSertExpVM.SingleExpenditureDetails = singleExp; 
        upSertExpVM.PageLoaded();
        UpSertExpbSheet.IsPresented = true;
        
    }

}
public class CustomSwitchRenderer: Microsoft.Maui.Controls.Switch
{
   
}
