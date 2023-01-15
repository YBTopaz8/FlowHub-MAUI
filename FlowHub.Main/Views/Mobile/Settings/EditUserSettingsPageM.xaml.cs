using FlowHub.Main.ViewModels.Settings;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Settings;

public partial class EditUserSettingsPageM : ContentPage
{
    private UserSettingsVM viewmodel;
	public EditUserSettingsPageM(UserSettingsVM vm)
	{
		InitializeComponent();
        viewmodel = vm;
        this.BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewmodel.PageLoadedCommand.Execute(null);
        viewmodel.GetCountryNamesList();
        CountryPicker.SelectedItem = viewmodel.ActiveUser.UserCountry;
    }

    private void CountryPicker_SelectedValueChanged(object sender, object e)
    {
        var pickedCountry = CountryPicker.SelectedItem;
        if (pickedCountry is not null)
        {
            viewmodel.CurrencyFromCountryPickedCommand.Execute(pickedCountry.ToString());
        }
    }
}