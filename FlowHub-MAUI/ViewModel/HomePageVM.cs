using Parse.Abstractions.Internal;
using Parse.LiveQuery;
using System.Diagnostics;

namespace FlowHub_MAUI.ViewModel;

public partial class HomePageVM : ObservableObject, IParseLiveQueryClientCallbacks
{
    [ObservableProperty]
    bool isAuthenticated;
    
    [ObservableProperty]
    ObservableCollection<FlowsModelView>? allFlows=new();
    [ObservableProperty]
    FlowsModelView? selectedFlow=new();
    
    [ObservableProperty]
    bool isBusy;
    [ObservableProperty]
    bool isRefreshing;
    [ObservableProperty]
    bool isFlowSelected;
    [ObservableProperty]
    bool isFlowDeleted;
    [ObservableProperty]
    bool isFlowAdded;
    [ObservableProperty]
    bool isFlowUpdated;
    [ObservableProperty]
    ObservableCollection<FlowCommentsView>? allCommentsForSpecificFlow=new();
    [ObservableProperty]
    ObservableCollection<FlowCommentsView>? allComments=new();
    [ObservableProperty]
    ObservableCollection<FlowModelAndCommentLink>? allFlowAndCommentsLinks=new();

    [ObservableProperty]
    FlowCommentsView singleFlowComment = new();
    [ObservableProperty]
    bool isConnectedOnline;
    [ObservableProperty]
    ParseUser? currentUserOnline=new();
    [ObservableProperty]
    UserModelView? currentUserLocal=new();

    [ObservableProperty]
    string? username;
    [ObservableProperty]
    string? userpassword;
    [ObservableProperty]
    string? useremail;

    public HomePageVM(IFlowsService flowsService)
    {
        FlowsService = flowsService;

    }
    async Task Testt()
    {

        var query = ParseClient.Instance.GetQuery("FlowCommentsView")
            .WhereEqualTo("IsDeleted", false);
        var results = await query.FindAsync();
        Debug.WriteLine($"Found {results.Count()} objects.");

    }
    public ParseLiveQueryClient? LiveClient { get; set; }

    [RelayCommand]
    public void QuickSignUp()
    {
        ParseUser signUpUser = new ParseUser();
        //signUpUser.Username = CurrentUserLocal.UserName;
        signUpUser.Email = "me@me.com";
        signUpUser.Username = "YBTopaz8";
        signUpUser.Password = "Yvan";
        ParseClient.Instance.SignUpAsync(signUpUser);
    }
    [RelayCommand]
    void SetupLiveQueries()
    {

        LiveClient = new ParseLiveQueryClient();
        SetupLiveQuery();
    }

    [RelayCommand]
    public void SyncFlows()
    {
        FlowsService.SyncAllData();
        if (FlowsService.AllFlows is not null)
        {
            AllFlows = FlowsService.AllFlows!.ToObservableCollection();
        }
        if (FlowsService.AllFlowComments is not null)
        {
            AllComments = FlowsService.AllFlowComments!.ToObservableCollection();
        }
        if (FlowsService.AllFlowAndCommentsLink is not null)
        {
            AllFlowAndCommentsLinks = FlowsService.AllFlowAndCommentsLink!.ToObservableCollection();
        }
    }

    public IFlowsService FlowsService { get; }

    [RelayCommand]
    public async Task LoginUser()
    {
        if (CurrentUserLocal is null)
        {
            CurrentUserLocal = new();
        }

        CurrentUserLocal.UserName= Username;
        CurrentUserLocal.UserPassword = Userpassword;

        ParseUser signUpUser = new ParseUser();
        //signUpUser.Username = CurrentUserLocal.UserName;
        signUpUser.Email = "me@me.com";
        signUpUser.Username = "YBTopaz8";
        signUpUser.Password = "Yvan";
        //signUpUser.Password = CurrentUserLocal.UserPassword;
        if (await FlowsService.LogUserIn(signUpUser, signUpUser.Password!))
        {
            //Debug.WriteLine("Login OK");

            var e = await ParseClient.Instance.CurrentUserController.GetCurrentSessionTokenAsync(ParseClient.Instance.Services);
            Debug.WriteLine(e);
            var ee = await ParseClient.Instance.GetCurrentSessionAsync();
            
            Debug.WriteLine(ee.SessionToken);
            Debug.WriteLine(ee.ObjectId);
            Debug.WriteLine(ee.Keys.FirstOrDefault());
        }
    }
    partial void OnCurrentUserOnlineChanged(ParseUser? oldValue, ParseUser? newValue)
    {
        Debug.WriteLine($"Old {oldValue?.IsAuthenticated} New {newValue?.IsAuthenticated}");
    }


    partial void OnCurrentUserOnlineChanging(ParseUser? oldValue, ParseUser? newValue)
    {
        if (newValue is not null)
        {
            if (newValue.IsAuthenticated)
            {
                
                //best to use this and away a little for parse client to validate auth
                IsAuthenticated = true;
                CurrentUserLocal.UserIDOnline = CurrentUserOnline.ObjectId;
                CurrentUserLocal.LocalDeviceId = CurrentUserOnline.ObjectId;
                CurrentUserLocal.UserPassword = Userpassword;
                CurrentUserLocal.IsAuthenticated = CurrentUserOnline.IsAuthenticated;
                CurrentUserLocal.UserEmail = CurrentUserLocal.UserEmail;
                CurrentUserLocal.UserName = CurrentUserLocal.UserName;

                FlowsService.UpdateUser(CurrentUserLocal);
                _ = Shell.Current.DisplayAlert("Login", "Login Successful", "OK");
            }
            else
            {
                IsAuthenticated = false;
            }
        }
    }
    [RelayCommand]
    public async Task SignUpUser()
    {
        if (CurrentUserLocal is null)
        {
            CurrentUserLocal = new();
        }

        CurrentUserLocal.UserName= Username;
        CurrentUserLocal.UserPassword = Userpassword;
        CurrentUserLocal.UserEmail = Useremail;

        ParseUser signUpUser= new ParseUser();
        signUpUser.Username = CurrentUserLocal.UserName;
        signUpUser.Password = CurrentUserLocal.UserPassword;
        signUpUser.Email = CurrentUserLocal.UserEmail;
         
        if (await FlowsService.SignUpUser(signUpUser))
        {
            Debug.WriteLine("Sign Up OK");
        }

    }
    [RelayCommand]

    public void AddFlow()
    {
        if (SingleFlowComment is not null)
        {
            SingleFlowComment.UserIDCommenting =  CurrentUserLocal!.UserIDOnline;
            
            
        }
        FlowsService.SaveBoth(SelectedFlow!, SingleFlowComment!);

        AllFlows!.Add(SelectedFlow!);
        SelectedFlow = new();
        Shell.Current.DisplayAlert("Add", "Flow Added Normal", "OK");
    }

    [RelayCommand]
    public void UpdateFlow()
    {
        FlowsService.UpdateFlow(SelectedFlow!);
        SelectedFlow = new();

        Shell.Current.DisplayAlert("Update", "Flow Updated Normal", "OK");
    }
    [RelayCommand]
    public void DeleteFlow()
    {
        FlowsService.UpdateFlow(SelectedFlow!);
        SelectedFlow = new();
    }
    public void AddFlowComment(FlowsModelView baseFlow)
    {
        AddFlow();
        SingleFlowComment = new();
    }
    [RelayCommand]
    public void QuickLogin()
    {
        _ =  LoginUser();
        return;

    }
    [RelayCommand]
    public void UpdateCommentUI()
    {
        FlowService.MapToParseObject(SingleFlowComment, nameof(FlowCommentsView)).SaveAsync();
        SingleFlowComment = new();
    }
    [RelayCommand]
    public void DeleteCommentUI()
    {
        SingleFlowComment.IsDeleted = true;
        FlowService.MapToParseObject(SingleFlowComment, nameof(FlowCommentsView)).SaveAsync();
        SingleFlowComment = new();

    }
    [RelayCommand]
    public void AddCommentUI()
    {
        SingleFlowComment.UserIDCommenting = CurrentUserLocal!.UserIDOnline;
        
        var comm = FlowService.MapToParseObject(SingleFlowComment, nameof(FlowCommentsView));
        comm["UserIDCommenting"] = "Yvan";
        _ = comm.SaveAsync();
        //SingleFlowComment = new();
    }
    //public void DeleteFlowComment(FlowsModelView baseFlow)
    //{

    //    if (SingleFlowComment is not null)
    //    {
    //        SingleFlowComment.UserIDCommenting = CurrentUserLocal!.UserIDOnline;
    //        SingleFlowComment.DateOfComment = DateTimeOffset.Now;
    //        FlowsService.(SingleFlowComment);
    //    }
    //    AllFlows!.Add(SelectedFlow!);
    //    FlowsService.UpdateFlow(baseFlow);

    //}

    [RelayCommand]
    public void UpdateUserAccount()
    {
        FlowsService.UpdateUser(CurrentUserLocal!);
    }
    [RelayCommand]
    public void LogoutUser()
    {
        FlowService.LogUserOut();
    }
    [ObservableProperty]
    List<string> flowCategory = new()
    {
        "Food",
        "Transportation",
        "Entertainment",
        "Healthcare",
        "Education",
        "Rent",
        "Other"
    };
    public ObservableCollection<ParseObject> Comments { get; private set; } = new ObservableCollection<ParseObject>();

    private async void SetupLiveQuery()
    {
        try
        {            
            var query = ParseClient.Instance.GetQuery("FlowCommentsView")
                //.WhereEqualTo("IsDeleted", false);
            var sub = LiveClient.Subscribe(query);
            LiveClient.RegisterListener(this);

            
            sub.HandleSubscribe(query =>
            {
                Debug.WriteLine($"Subscribed to query: {query.GetClassName()}");
            })
 .HandleEvents((query, objEvent, obj) =>
 {
     Debug.WriteLine($"Event {objEvent} occurred for object: {obj.ObjectId}");
 })
 .HandleError((query, exception) =>
 {
     Debug.WriteLine($"Error in query for class {query.GetClassName()}: {exception.Message}");
 })
 .HandleUnsubscribe(query =>
 {
     Debug.WriteLine($"Unsubscribed from query: {query.GetClassName()}");
 });

            // Connect asynchronously
          await Task.Run(()=>  LiveClient.ConnectIfNeeded());
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SetupLiveQuery encountered an error: {ex.Message}");
        }
    }

    public void OnLiveQueryClientConnected(ParseLiveQueryClient client)
    {
        Debug.WriteLine("Client Connected");
    }

    public void OnLiveQueryClientDisconnected(ParseLiveQueryClient client, bool userInitiated)
    {
        Debug.WriteLine("Client Disconnected");
    }

    public void OnLiveQueryError(ParseLiveQueryClient client, LiveQueryException reason)
    {
        Debug.WriteLine("Error "+reason.Message);
    }

    public void OnSocketError(ParseLiveQueryClient client, Exception reason)
    {
        Debug.WriteLine("Socket Error ");
    }
}
