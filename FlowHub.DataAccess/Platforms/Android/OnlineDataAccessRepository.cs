namespace FlowHub.DataAccess;

public class OnlineDataAccessRepository : IOnlineCredentialsRepository
{
    public IMongoDatabase OnlineMongoDatabase { get; set; }
    public void GetOnlineConnection()
    {
        if (Connectivity.Current.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            IMongoDatabase db = new MongoClient("<< YOUR ATLAS CONNECTION STRING >>")
                .GetDatabase("<<YOUR DB NAME>>");
            OnlineMongoDatabase = db;
        }
        else
        {
            _ = Shell.Current.DisplayAlert("No Internet Found!", "Please Connect your devcice to the internet", "OK");
        }
    }
}