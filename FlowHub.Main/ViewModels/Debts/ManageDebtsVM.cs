using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.ViewModels.Debts;

public partial class ManageDebtsVM : ObservableObject
{
    private readonly IDebtRepository debtRepo;
    private readonly IUsersRepository usersRepo;

    public ManageDebtsVM(IDebtRepository debtRepository, IUsersRepository usersRepository)
    {
        debtRepo = debtRepository;
        usersRepo = usersRepository;
        debtRepo.OfflineDebtListChanged += HandleDebtsListUpdated;
    }
    [ObservableProperty]
    ObservableCollection<DebtModel> debtsList;
    [ObservableProperty]
    int totalDebts;
    [ObservableProperty]
    int totalLent;
    [ObservableProperty]
    int totalBorrowed;
    [ObservableProperty]
    UsersModel activeUser;

    bool IsLoaded;
    public void GetAllDebts()
    {
        try
        {
            if (IsLoaded)
            {
                ApplyChanges();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when loading all debts MESSAGE : {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task GoToAddDebtAsync()
    {
        var navParams = new Dictionary<string, object>()
        {
            {"SingleDebtDetails", new DebtModel()
            {
              Amount = 0,
              PersonOrOrganization = new PersonOrOrganizationModel(),
              Currency = usersRepo.OfflineUser.UserCurrency
            } },
            {"ActiveUser", ActiveUser },
            {"PageTitle", "Add Debt" }
        };
        await Shell.Current.GoToAsync(nameof(UpSertDebtPageM),true, navParams);
    }

    private void ApplyChanges()
    {
        List<DebtModel> debtList = new();
        debtList = debtRepo.OfflineDebtList
                    .OrderByDescending(x => x.UpdateDateTime)
                    .ToList();
        DebtsList = new ObservableCollection<DebtModel>(debtList);

        TotalBorrowed = debtList.Count(x => x.DebtType == DebtType.Borrowed);//.Sum(x => x.Amount);
        TotalLent = debtList.Count(x => x.DebtType == DebtType.Lent);//.Sum(x => x.Amount);
    }

    private void HandleDebtsListUpdated()
    {
        ApplyChanges();
    }
}