namespace FlowHub_MAUI.Utilities.Models;

public partial class FlowsModel : RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public string? UserID { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset? DeadLine { get; set; }    
    public string? ReceiptImageUrl { get; set; }    
    public bool IsFlowIn { get; set; }=false; // FlowIn = Income or FlowOut = Expense
    public bool IsDebt { get; set; }=false; 
    public bool IsRecurring { get; set; }=false;
    public int RecurrenceNumber { get; set; }=0;
    public string? ForUserID { get; set; }
    public string? Description { get; set; }
    public double Amount { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; } = "Card";
    public string? DateCreated { get; set; } = DateTime.UtcNow.ToString("o");
    public string? DeviceName { get; set; } = DeviceInfo.Current.Name;
    public string? DeviceFormFactor { get; set; } = DeviceInfo.Current.Idiom.ToString();
    public string? DeviceModel { get; set; } = DeviceInfo.Current.Model;
    public string? DeviceManufacturer { get; set; } = DeviceInfo.Current.Manufacturer;
    public string? DeviceVersion { get; set; } = DeviceInfo.Current.VersionString;
    // Backing field for the category
    public string? Category { get; set; }
    // ACL for sharing permissions
    [Ignored]
    public ParseACL Acl { get; set; } = new ParseACL();
    public string? FlowBaseID { get; set; }
    public string? FlowModelCommentLinkID { get; set; }
    public IList<string>? Tags { get; }
    public FlowsModel()
    {
        
    }
    public FlowsModel(FlowsModel model)
    {
        
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
        ReceiptImageUrl = model.ReceiptImageUrl;        
        
        FlowBaseID = model.FlowBaseID;
        IsDebt = model.IsDebt;
        IsFlowIn = model.IsFlowIn;
    }
    public FlowsModel(FlowsModelView model)
    {
        LocalDeviceId = model.LocalDeviceId;
        Category = model.Category;
        DeadLine = model.DeadLine;
        ReceiptImageUrl = model.ReceiptImageUrl;
        FlowModelCommentLinkID = model.FlowModelCommentLinkID;

        Tags = model.Tags.Select(x=>x).ToList();
        IsDebt = model.IsDebt;
        IsFlowIn= model.IsFlowIn;
        
        
    }
    // Override Equals to compare based on string
    public override bool Equals(object? obj)
    {
        if (obj is FlowsModelView other)
        {
            return this.LocalDeviceId == other.LocalDeviceId;
        }
        return false;
    }

    // Override GetHashCode to use string's hash code
    public override int GetHashCode()
    {
        return LocalDeviceId!.GetHashCode();
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


public partial class FlowModelAndCommentLink:RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public string? FlowID { get; set; }
    public string? CommentID { get; set; }
    public FlowModelAndCommentLink()
    {

    }


}


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
    string? flowModelCommentLinkID;
    [ObservableProperty]
    string? location;
    [ObservableProperty]
    List<string> tags = new();
    [ObservableProperty]
    bool isRecurring = false;
    [ObservableProperty]
    string? recurringInterval; // Could be Enum-backed

    [ObservableProperty]
    string? forUserID;

    [ObservableProperty]
    string? description;

    [ObservableProperty]
    double amount;

    [ObservableProperty]
    string? dateCreated= DateTime.UtcNow.ToString("o");
    [ObservableProperty]
    string? notes;
    [ObservableProperty]
    string? paymentMethod="Card";
    [ObservableProperty]
    ParseACL? aCL;

    public FlowsModelView()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            if (ParseClient.Instance is not null)
            {
                var currentUser = ParseClient.Instance.GetCurrentUser();
                if (currentUser is not null)
                {
                    ACL = new();
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
        ReceiptImageUrl = model.ReceiptImageUrl;
        
        FlowModelCommentLinkID = model.FlowModelCommentLinkID;
        IsDebt = model.IsDebt;
        IsRecurring = model.IsRecurring;

        IsFlowIn = model.IsFlowIn;

        ForUserID = model.ForUserID;
        Description = model.Description;
        Amount = model.Amount;
        DateCreated = model.DateCreated;
        Notes = model.Notes;
        PaymentMethod = model.PaymentMethod;

        if (model.Tags is not null)
        {
            Tags = model.Tags.ToList();
        }
        model.FlowBaseID = model.FlowBaseID;
    }
    // Override Equals to compare based on string
    public override bool Equals(object? obj)
    {
        if (obj is FlowsModelView other)
        {
            return this.LocalDeviceId == other.LocalDeviceId;
        }
        return false;
    }

    // Override GetHashCode to use string's hash code
    public override int GetHashCode()
    {
        return LocalDeviceId!.GetHashCode();
    }
}
    // Method to map back to FlowsModel for saving to the database

public partial class FlowComments: RealmObject
{
    [PrimaryKey]
    public string? LocalDeviceId { get; set; } = GeneralStaticUtilities.GenerateRandomString(nameof(FlowsModelView));
    public string? UserIDCommenting { get; set; }
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
        LocalDeviceId = model.LocalDeviceId;
        UserIDCommenting = model.UserIDCommenting;
        Comment = model.Comment;
        
        IsDeleted = model.IsDeleted;
    }
}

public partial class FlowCommentsView: ObservableObject
{
    [ObservableProperty]
    string? localDeviceId = GeneralStaticUtilities.GenerateRandomString(nameof(FlowCommentsView));
    [ObservableProperty]
    string? userIDCommenting;
    [ObservableProperty]
    string? comment;
    [ObservableProperty]
    bool isDeleted;

    public FlowCommentsView()
    {
        
    }
    public FlowCommentsView(FlowComments model)
    {
        LocalDeviceId = model.LocalDeviceId;
        UserIDCommenting = model.UserIDCommenting!;
        Comment = model.Comment;
        
        IsDeleted = model.IsDeleted;
    }
}