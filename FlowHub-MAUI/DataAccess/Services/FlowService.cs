namespace FlowHub_MAUI.DataAccess.Services;

public class FlowService : IFlowsService
{
    Realm? db;
    public IList<FlowsModelView>? AllFlows { get; set ; }
    HomePageVM? ViewModel { get; set; }
    public IDataBaseService DataBaseService { get; }

    public FlowService(IDataBaseService dataBaseService)
    {
        DataBaseService = dataBaseService;

        GetUserAccount();

    }

    bool HasOnlineSyncOn;
    public ParseUser? CurrentUserOnline { get; set; }

    public UserModelView CurrentOfflineUser { get; set; }
    public void InitApp(HomePageVM vm)
    {
        ViewModel = vm;
    }

    public UserModelView? GetUserAccount(ParseUser? usr = null)
    {
        if (CurrentOfflineUser is not null && CurrentOfflineUser.IsAuthenticated && usr == null)
        {
            return CurrentOfflineUser;
        }
        db = Realm.GetInstance(DataBaseService.GetRealm());
        var dbUser = db.All<UserModel>().ToList().FirstOrDefault();

        if (dbUser == null)
        {
            if (usr is not null)
            {
                CurrentOfflineUser = new UserModelView(usr);
                db.Write(() =>
                {
                    UserModel user = new(CurrentOfflineUser);

                    db.Add(user, true);
                });
                return CurrentOfflineUser;

            };

            CurrentOfflineUser = new UserModelView()
            {
                UserName = "User",
                UserEmail = "user@FlowHub.com",
                UserPassword = "1234",

            };
            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                UserModel user = new(CurrentOfflineUser);
                db.Add(user);
            });
            return CurrentOfflineUser;
        }
        CurrentOfflineUser = new(dbUser);
        return CurrentOfflineUser;
    }

    public void GetFlows()
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            AllFlows?.Clear();
            var realflows = db.All<FlowsModel>().OrderBy(x=>x.DateCreated).ToList();
            AllFlows = new List<FlowsModelView>(realflows.Select(flow => new FlowsModelView(flow)));
            AllFlows ??= Enumerable.Empty<FlowsModelView>().ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public bool UpdateFlow(FlowsModelView flow)
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                var existingFlow = db.All<FlowsModel>()
                .Where(x => x.LocalDeviceId == flow.LocalDeviceId)
                .ToList();

                if (existingFlow.Count < 1)
                {
                    FlowsModel floww = new FlowsModel(flow);
                    db.Add(floww);                    
                }
                if (existingFlow?.Count > 0)
                {
                    var flow = existingFlow.First();
                    var ee = new FlowsModelView(flow);
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when updating song: " + ex.Message);
            return false;
        }
    }
}
