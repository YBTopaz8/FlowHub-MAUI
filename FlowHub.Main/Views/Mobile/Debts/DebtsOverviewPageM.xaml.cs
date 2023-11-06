namespace FlowHub.Main.Views.Mobile.Debts;

public partial class DebtsOverviewPageM : UraniumContentPage
{
    readonly ManageDebtsVM viewModel;
    readonly UpSertDebtVM UpSertVM;

    public DebtsOverviewPageM(ManageDebtsVM vm, UpSertDebtVM upSertDebt)
    {
        InitializeComponent();
        viewModel = vm;
        UpSertVM = upSertDebt;
        BindingContext = vm;
        bottomSheet.BindingContext = UpSertVM;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
    }


    private async void LentBrdr_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ManageLendingsPageM), true);
    }

    private async void BorrowBrdr_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ManageBorrowingsPageM), true);
    }

    private void AmountTextField_Focused(object sender, FocusEventArgs e)
    {
        if (AmountTextField.Text == "1")
        {
            AmountTextField.Text = "";
        }
    }
    private void AddNewFlowHoldBtn_Clicked(object sender, EventArgs e)
    {
        UpSertVM.SingleDebtDetails = new DebtModel()
        {
            Amount = 1,
            PersonOrOrganization = new PersonOrOrganizationModel(),
            Currency = viewModel.UserCurrency,
            

        };

        UpSertVM.PageLoaded();
        bottomSheet.IsPresented = true;
        
    }

    private void DeadlineSwitch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

        if(e.PropertyName == "IsToggled")
        {            
            FlowHoldDeadline.Date = DateTime.Now;
        }
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            }

            var PickedContact = await Contacts.Default.PickContactAsync();
            if (PickedContact is null)
            {
                Debug.WriteLine("Contact not picked");
            }

            string namePrefix = PickedContact.DisplayName;
            PersonName.Text = namePrefix;
            PersonNumber.Text = PickedContact.Phones.FirstOrDefault()?.PhoneNumber;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Permission denied " + ex.Message);
        }
    }

    /*
    private async void SearchBarViewToggler_Clicked(object sender, EventArgs e)
    {
        if (SearchBarView.IsVisible)
        {
            // Hide SearchBarView and move RestOfPage back to original position
            var searchBarFadeOutTask = SearchBarView.FadeTo(0, 400);
            var restOfPageMoveUpTask = RestOfPage.TranslateTo(0, 0, 400);

            await Task.WhenAll(searchBarFadeOutTask, restOfPageMoveUpTask);

            SearchBarView.IsVisible = false;
            RestOfPage.TranslationY = 0; // Reset position
        }
        else
        {
            SearchBarView.IsVisible = true;

            // Initialize position and opacity for the animation
            SearchBarView.Opacity = 0;
            RestOfPage.TranslationY = 0; // Start from original position

            // Show SearchBarView and move RestOfPage down
            var searchBarFadeInTask = SearchBarView.FadeTo(1, 400);
            var restOfPageMoveDownTask = RestOfPage.TranslateTo(0, SearchBarView.Height-200, 400);

            await Task.WhenAll(searchBarFadeInTask, restOfPageMoveDownTask);

            RestOfPage.TranslationY = SearchBarView.Height; // Ensure it stays at the intended position
        }
    
    }*/


}