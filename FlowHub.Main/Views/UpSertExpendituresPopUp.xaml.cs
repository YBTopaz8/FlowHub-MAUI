using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views;

public partial class UpSertExpendituresPopUp : Popup
{
	readonly UpSertExpenditureVM viewModel;
	public UpSertExpendituresPopUp(UpSertExpenditureVM vm)
	{
		InitializeComponent();
		viewModel = vm;
        BindingContext = vm;
        viewModel.PageLoaded();

        //#if ANDROID
        //        var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        //        var orientation = mainDisplayInfo.Orientation;
        //        var width = mainDisplayInfo.Width;
        //        var height = mainDisplayInfo.Height;
        //        var density = mainDisplayInfo.Density;
        //        var smallestWidth = Math.Min(width, height) / density;
        //        var popupWidth = smallestWidth * 0.9;

        //        double popupHeight = 0;
        //        if (smallestWidth < 415)
        //        {
        //            if ((smallestWidth * 0.13) > 401)
        //            {
        //                popupHeight = 350;
        //            }
        //            else
        //            {
        //                popupHeight = (double)(smallestWidth * 0.92);
        //            }

        //        }
        //        //else if(smallestWidth)

        //        Size = new Size(popupWidth, popupHeight);
#if WINDOWS
        this.Size = new Size(400, 420);
#endif
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ClosePopUp) && viewModel.ClosePopUp)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (viewModel.ThisPopUpResult == PopupResult.Cancel)
            {
                Close(new PopUpCloseResult() { Data = null, Result = PopupResult.Cancel });
                return;
            }
            Close(new PopUpCloseResult() { Data = viewModel.SingleExpenditureDetails, Result = viewModel.ThisPopUpResult});
        }
    }

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        var s = sender as TextField;
        if (s.Text?.Length == 0)
        {
            viewModel.SingleExpenditureDetails.UnitPrice = 0;
        }

        viewModel.UnitPriceOrQuantityChanged();
    }
    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
        }
    }
}
