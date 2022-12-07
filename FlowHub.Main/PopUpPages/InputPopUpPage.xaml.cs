using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace FlowHub.Main.PopUpPages;

public partial class InputPopUpPage : Popup
{
    bool IsNumericInput;
    bool IsTextInput;
    string TitleText;
	public InputPopUpPage(bool isNumericInput = false, bool isTextInput=false, bool isEmailInput=false, string optionalTitleText=null)
	{
		InitializeComponent();

        IsNumericInput= isNumericInput;
        IsTextInput= isTextInput;
        TitleText= optionalTitleText;

        if (IsNumericInput)
        {
            InputAmount.IsVisible= true;
            InputAmount.Title = TitleText is null ? InputAmount.Title : TitleText;
        }
        if (IsTextInput)
        {
            InputText.IsVisible= true;
            InputText.Title = TitleText is null ? InputText.Title : TitleText;
        }
	}

    
    void OnOKButtonClicked(object sender, EventArgs e)
    {
		if (IsNumericInput)
		{
            double amount = double.Parse(InputAmount.Text);
            Close(amount);
        }

        if(IsTextInput)
        {            
            Close(InputText.Text);
        }
    }

    void OnCancelButtonClicked(object sender, EventArgs e)
    {
        if (IsNumericInput)
        {
            Close(0d);
        }
        if (IsTextInput)
        {
            Close("");
        }
    }
   
}