namespace FlowHub.Main.PopUpPages;

public partial class ErrorPopUpAlert : Popup
{
    public ErrorPopUpAlert(string ErrorText = "Nothing")
    {
        InitializeComponent();
        DisplayErrorText.Text = ErrorText;
    }

    void OnOKButtonClicked(object sender, EventArgs e) => Close();
}