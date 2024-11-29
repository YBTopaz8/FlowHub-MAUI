namespace FlowHub_MAUI.Utilities.Models;

public partial class UserModel : RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public string? Username { get; set; }    
    public string? Email {get;set;}    
    public string? Password {get;set;}    
    public string? CoverImage {get;set;}      
    public string? PhoneNumber {get;set;}    
    public string? Address {get;set;}    
    public string? City {get;set;}    
    public string? State {get;set;}    
    public string? Currency {get;set; }

    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    public UserModel()
    {
        
    }

    public UserModel(UserModelView model)
    {
        LocalDeviceId = model.LocalDeviceId;
        Username = model.UserName;
        Email = model.UserEmail;
        Password = model.UserPassword;
        CoverImage = model.CoverImage;
        PhoneNumber = model.PhoneNumber;
        Address = model.Address;
        City = model.City;
        State = model.State;
        Currency = model.Currency;

    }
}

public partial class UserModelView : ObservableObject
{

    [ObservableProperty]
    string? localDeviceId = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));

    [ObservableProperty]
    bool isAuthenticated;
    [ObservableProperty]
    string? userIDOnline;
    [ObservableProperty]
    string? userName = "User";
    [ObservableProperty]
    string? userPassword;
    [ObservableProperty]
    string? userEmail;
    [ObservableProperty]
    string? coverImage = string.Empty;
    [ObservableProperty]
    DateTimeOffset lastSessionDate;

    [ObservableProperty]
    string? phoneNumber;
    [ObservableProperty]
    string? address;
    [ObservableProperty]
    string? city;
    [ObservableProperty]
    string? state;
    [ObservableProperty]
    string? currency;


    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    public UserModelView()
    {
        
    }
    public UserModelView(UserModel model)
    {
        LocalDeviceId = model.LocalDeviceId;
        UserName = model.Username;
        UserEmail= model.Email;
        UserPassword = model.Password;
        CoverImage = model.CoverImage;
        PhoneNumber = model.PhoneNumber;
        Address = model.Address;
        City = model.City;
        State = model.State;
        Currency = model.Currency;
        
    }

    public UserModelView(ParseUser model)
    {
        UserEmail = model.Email;
        UserName = model.Username;
        UserIDOnline = model.ObjectId;
        IsAuthenticated = model.IsAuthenticated;
    }
}

public partial class ReportModel : RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));

    public UserModel? User { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportDescription { get; set; }
    public DateTimeOffset Date { get; set; }
    public double ReportTotalAmount { get; set; }
    public double ReportCurrency { get; set; }

    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    public ReportModel()
    {

    }

    public ReportModel(ReportModelView model)
    {
        LocalDeviceId = model.LocalDeviceId;
        User = model.User;
        ReportTitle = model.ReportTitle;
        ReportDescription = model.ReportDescription;
        Date = model.Date;
        ReportTotalAmount = model.ReportTotalAmount;
        ReportCurrency = model.ReportCurrency;
    }
}

public partial class ReportModelView : ObservableObject
{
    [ObservableProperty]
    string? localDeviceId = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));

    [ObservableProperty]
    UserModel? user;

    [ObservableProperty]
    string? reportTitle;

    [ObservableProperty]
    string? reportDescription;

    [ObservableProperty]
    DateTimeOffset date;

    [ObservableProperty]
    double reportTotalAmount;
    [ObservableProperty]
    double reportCurrency;
    public ReportModelView()
    {

    }

    public ReportModelView(ReportModel model)
    {
        LocalDeviceId = model.LocalDeviceId;
        User = model.User;
        ReportTitle = model.ReportTitle;
        ReportDescription = model.ReportDescription;
        Date = model.Date;
        ReportTotalAmount = model.ReportTotalAmount;
        ReportCurrency = model.ReportCurrency;
        //ReportComments = model.ReportComments.Select(x => new ReportComments(x)).ToList();
    }
}