using CommunityToolkit.Maui.Views;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.PopUpPages;

public partial class InputPopUpPage : Popup
{
    public InputType GivenInputType { get; set; }
	public InputPopUpPage(InputType inputType, List<string> FieldsTitlesTextThenNumeric, string optionalPopUpTitleText=null, TaxModel SelectedTax = null, bool IsDeleteBtnVisible = false)
	{
		InitializeComponent();

        DeleteButton.IsVisible = IsDeleteBtnVisible;
        if (SelectedTax is not null)
        {
            InputText.Text = SelectedTax.Name;
            InputAmount.Text = SelectedTax.Rate.ToString();
        }

        GivenInputType = inputType;

        PopUpTitle.Text= optionalPopUpTitleText;

        if (GivenInputType.HasFlag(InputType.Numeric) && GivenInputType.HasFlag(InputType.Text))
        {
            InputText.IsVisible = true;
            InputText.Title = FieldsTitlesTextThenNumeric[0];

            InputAmount.IsVisible = true;
            InputAmount.Title = FieldsTitlesTextThenNumeric[1];

            this.Size = new Size(300, 260);
        }
        else if (GivenInputType.HasFlag(InputType.Numeric))
        {
            InputAmount.IsVisible = true;
            InputAmount.Title = FieldsTitlesTextThenNumeric[0];
            this.Size = new Size(300, 200);
        }
        else if (GivenInputType.HasFlag(InputType.Text))
        {
            InputText.IsVisible = true;
            InputText.Title = FieldsTitlesTextThenNumeric[0];
        }
        else
        {
            Debug.WriteLine("InputPopUpPage: GivenInputType is not valid");
        }
	}

    void OnOKButtonClicked(object sender, EventArgs e)
    {
        if (GivenInputType.HasFlag(InputType.Numeric) && GivenInputType.HasFlag(InputType.Text))
        {
            string taxName = InputText.Text;
            double taxAmount = double.Parse(InputAmount.Text);
            Dictionary<string, double> dict = new()
            {
                { taxName,taxAmount }
            };

            Close(new PopUpCloseResult() { Data = dict, Result = PopupResult.OK });
        }
        else if (GivenInputType.HasFlag(InputType.Numeric))
        {
            double amount = double.Parse(InputAmount.Text);

            Close(new PopUpCloseResult() { Data = amount, Result = PopupResult.OK });
        }
        else if (GivenInputType.HasFlag(InputType.Text))
        {
            Close(new PopUpCloseResult() { Data = InputText.Text, Result = PopupResult.OK });
        }
    }

    void OnCancelButtonClicked(object sender, EventArgs e)
    {
        Close(new PopUpCloseResult() { Data = null, Result = PopupResult.Cancel });
    }
    private void OnDeleteButton_Clicked(object sender, EventArgs e)
    {
        Close(new PopUpCloseResult() { Data = null, Result = PopupResult.Delete });
    }
}

public class PopUpCloseResult
{
    public object Data { get; set; }
    public PopupResult Result { get; set; }
}
public enum PopupResult
{
    OK,
    Cancel,
    Delete
}
[Flags]
public enum InputType
{
    None = 0,
    Numeric = 1,
    Text = 2,
    Email = 4
}