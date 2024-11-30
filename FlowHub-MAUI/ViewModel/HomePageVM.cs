using Parse.Abstractions.Internal;
using Parse.LiveQuery;

namespace FlowHub_MAUI.ViewModel;

public partial class HomePageVM : ObservableObject
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

    public ParseLiveQueryClient LiveClient { get; set; }

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
        signUpUser.Username = CurrentUserLocal.UserName;
        signUpUser.Password = CurrentUserLocal.UserPassword;
        if (await FlowsService.LogUserIn(signUpUser, signUpUser.Password!))
        {
            //Debug.WriteLine("Login OK");
            CurrentUserOnline = ParseClient.Instance.GetCurrentUser();
            CurrentUserLocal.UserIDOnline = CurrentUserOnline.ObjectId;
            CurrentUserLocal.LocalDeviceId = CurrentUserOnline.ObjectId;
            CurrentUserLocal.UserPassword = Userpassword;
            
            FlowsService.UpdateUser(CurrentUserLocal);
        }
    }



    partial void OnCurrentUserOnlineChanging(ParseUser? oldValue, ParseUser? newValue)
    {
        if (newValue is not null)
        {
            if (newValue.IsAuthenticated)
            {
                IsAuthenticated = true;
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
    }

    [RelayCommand]
    public void UpdateFlow()
    {
        FlowsService.UpdateFlow(SelectedFlow!);
        SelectedFlow = new();
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

    
    public void AddCommentUI()
    {
        
        SingleFlowComment.UserIDCommenting = CurrentUserLocal!.UserIDOnline;
        SingleFlowComment.Comment = SingleFlowComment.Comment + " Sent from "+ DeviceInfo.Idiom;
        var comm = FlowService.MapToParseObject(SingleFlowComment, nameof(FlowCommentsView));
        _ = comm.SaveAsync();
        SingleFlowComment = new();
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
                .WhereEqualTo("key", "value");

            var sub = LiveClient.Subscribe(query);

            sub
                .HandleSubscribe(query =>
                {
                    Debug.WriteLine($"Successfully subscribed to query: {query.GetClassName()}");
                })
                .HandleUnsubscribe(query =>
                {
                    Debug.WriteLine($"Unsubscribed from query: {query}");
                })
                .HandleEvents((query, objEvent, obj) =>
                {
                    var obb = FlowService.MapToModelFromParseObject<FlowCommentsView>((ParseObject)obj);
                    Debug.WriteLine($"Event {objEvent} occurred for object: {obj.ObjectId}");
                    switch (objEvent)
                    {
                        case Subscription.Event.Create:
                            //FlowModelAndCommentLink? itemmm = MapToModelFromParseObject<FlowModelAndCommentLink>((ParseObject)item); //duration is off

                            AllComments!.Add(obb); // Add new object
                            break;
                        case Subscription.Event.Update:
                            
                            break;
                        case Subscription.Event.Delete:
                            AllComments!.Remove(obb);
                            Debug.WriteLine($"Object deleted: {obj.ObjectId}");
                            break;
                    }
                })
                .HandleError((query, exception) =>
                {
                    Debug.WriteLine($"Error encountered: {exception.Message}");
                });

            // Connect asynchronously
            await Task.Run(() => LiveClient.ConnectIfNeeded());
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

}
