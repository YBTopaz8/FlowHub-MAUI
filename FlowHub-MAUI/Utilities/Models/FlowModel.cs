namespace FlowHub_MAUI.Utilities.Models;

public partial class FlowsModel : RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public UserModel? User { get; set; }
    public string? Description { get; set; }
    public double Amount { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset? DeadLine { get; set; }
    public string? ReceiptImageThumbnailPath { get; set; }
    public string? ReceiptImageUrl { get; set; }
    public string? ReceiptImageThumbnailUrl { get; set; }
    public string? Notes { get; set; }
    public bool IsFlowIn { get; set; }=false; // FlowIn = Income or FlowOut = Expense
    public bool IsDebt { get; set; }=false; 
    public bool IsRecurring { get; set; }=false;

    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    // Backing field for the category
    private string? category;
    public string? Category
    {
        get => category;
        set
        {
            category = value;
            _ = Enum.TryParse<FlowCategory>(category, out var parsed);
            InternalCategory = parsed;
        }
    }
    [Ignored] // Not stored in the database
    public FlowCategory InternalCategory { get; set; }

    // ACL for sharing permissions
    [Ignored]
    public ParseACL Acl { get; set; } = new ParseACL();
    public FlowModelBase? FlowBase { get; set; }
    public IList<FlowComments>? AllFlowComments { get; }
    public IList<string>? Tags { get; }
    public FlowsModel()
    {
        
    }
    public FlowsModel(FlowsModel model)
    {
        if (AllFlowComments is not null)
        {
            AllFlowComments.Clear();
            if (model.AllFlowComments is not null)
            {
                var s = model.AllFlowComments.ToList();
                foreach (var item in s)
                {
                    var q = item;
                    AllFlowComments.Add(q);
                }
            }

        }
        if (Tags is not null)
        {
            Tags.Clear();
            if (model.Tags is not null)
            {
                var s = model.Tags.ToList();
                foreach (var item in s)
                {
                    var q = item;
                    Tags.Add(q);
                }
            }

        }

        LocalDeviceId = model.LocalDeviceId;
        Category = model.Category;
        DeadLine = model.DeadLine;
        ReceiptImageThumbnailPath = model.ReceiptImageThumbnailPath;
        ReceiptImageUrl = model.ReceiptImageUrl;
        ReceiptImageThumbnailUrl = model.ReceiptImageThumbnailUrl;
        
        FlowBase = model.FlowBase;
        IsDebt = model.IsDebt;
        IsFlowIn = model.IsFlowIn;
    }
    public FlowsModel(FlowsModelView model)
    {
        LocalDeviceId = model.LocalDeviceId;
        Category = model.Category;
        DeadLine = model.DeadLine;
        ReceiptImageThumbnailPath = model.ReceiptImageThumbnailPath;
        ReceiptImageUrl = model.ReceiptImageUrl;
        ReceiptImageThumbnailUrl = model.ReceiptImageThumbnailUrl;
        AllFlowComments = model.AllFlowComments.Select(x => new FlowComments(x)).ToList();
        FlowBase = new(model?.FlowBase);
        Tags = model.Tags.Select(x=>x).ToList();
        IsDebt = model.IsDebt;
        IsFlowIn= model.IsFlowIn;
    }
}


/*
 * 
Allow User B to comment
flow.Acl.SetWriteAccess(userB, true);

// Save the flow
realm.Write(() => realm.Add(flow));
await flow.SaveAsync(); // Parse sync
 * 
 * 
 * 
 * */




public partial class FlowsModelView : ObservableObject
{
    [ObservableProperty]
    string? localDeviceId = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    [ObservableProperty]
    string? category;
    [ObservableProperty]
    DateTimeOffset? deadLine;
    [ObservableProperty]
    string? receiptImageThumbnailPath;
    [ObservableProperty]
    string? receiptImageUrl;
    [ObservableProperty]
    string? receiptImageThumbnailUrl;
    [ObservableProperty]
    bool isDebt = false;    
    [ObservableProperty]
    bool isFlowIn= false;
    [ObservableProperty]
    List<FlowCommentsView> allFlowComments = new List<FlowCommentsView>();
    [ObservableProperty]
    string? location;
    [ObservableProperty]
    List<string> tags = new();
    [ObservableProperty]
    bool isRecurring = false;
    [ObservableProperty]
    string? recurringInterval; // Could be Enum-backed
    [ObservableProperty]
    FlowModelBaseView? flowBase;

    [ObservableProperty]
    ParseACL aCL = new();

    public FlowsModelView()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            if (ParseClient.Instance is not null)
            {
                var currentUser = ParseClient.Instance.GetCurrentUser();
                if (currentUser is not null)
                {
                    ACL.SetReadAccess(currentUser.ObjectId, true);
                    ACL.SetWriteAccess(currentUser.ObjectId, true);
                    
                }

            }

        }
    }
    public FlowsModelView(FlowsModel model)
    {
        LocalDeviceId = model.LocalDeviceId;

        Category = model.Category;

        DeadLine = model.DeadLine;
        ReceiptImageThumbnailPath = model.ReceiptImageThumbnailPath;
        ReceiptImageUrl = model.ReceiptImageUrl;
        ReceiptImageThumbnailUrl = model.ReceiptImageThumbnailUrl;
        AllFlowComments = model.AllFlowComments?.Select(x => new FlowCommentsView(x)).ToList()!;
        FlowBase = new(model.FlowBase);
        IsDebt = model.IsDebt;
        IsRecurring = model.IsRecurring;
        Tags = model.Tags.ToList();
        IsFlowIn = model.IsFlowIn;

    }
    // Method to map back to FlowsModel for saving to the database

}
public partial class FlowModelBase: RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowModelBase));
    public UserModel? ForUser { get; set; }
    public string? Description{ get; set; }
    public double Amount { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; } = "Card";


    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    public FlowModelBase()
    {
        
    }
    public FlowModelBase(FlowModelBaseView model)
    {
        LocalDeviceId = model.LocalDeviceId;
        ForUser = model.ForUser;
        Description = model.Description;
        Amount = model.Amount;
        DateCreated = model.DateCreated;
        Notes = model.Notes;
        PaymentMethod = model.PaymentMethod;
    }
}
public partial class FlowModelBaseView : ObservableObject
{
    [ObservableProperty]
    string? localDeviceId = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));

    [ObservableProperty]
    UserModel? forUser;

    [ObservableProperty]
    string? description;

    [ObservableProperty]
    double amount;

    [ObservableProperty]
    string? dateCreated;
    [ObservableProperty]
    string? notes;
    [ObservableProperty]
    string? paymentMethod="Card";

    public FlowModelBaseView()
    {
        
    }
    public FlowModelBaseView(FlowModelBase model)
    {
        LocalDeviceId = model.LocalDeviceId;
        ForUser = model.ForUser;
        Description = model.Description;
        Amount = model.Amount;
        DateCreated = model.DateCreated;
        Notes = model.Notes;
        PaymentMethod = model.PaymentMethod;

    }
}



public partial class FlowComments: RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public UserModel? UserCommenting { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset DateOfComment { get; set; }=DateTimeOffset.Now;
    public bool IsDeleted { get; set; } = false;


    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    public FlowComments()
    {
        
    }
    public FlowComments(FlowCommentsView model)
    {
        UserCommenting = model.UserCommenting;
        Comment = model.Comment;
        DateOfComment = model.DateOfComment;
    }
}

public partial class FlowCommentsView: ObservableObject
{
    [ObservableProperty]
    UserModel? userCommenting;
    [ObservableProperty]
    string? comment;
    [ObservableProperty]
    bool isDeleted;
    [ObservableProperty]
    DateTimeOffset dateOfComment ;

    public FlowCommentsView()
    {
        
    }
    public FlowCommentsView(FlowComments model)
    {
        UserCommenting = model.UserCommenting!;
        Comment = model.Comment;
        DateOfComment = model.DateOfComment;
        IsDeleted = model.IsDeleted;
    }
}

public enum FlowCategory
{
    Food,
    Transportation,
    Entertainment,
    Healthcare,
    Education,
    Other
}
