namespace FlowHub.DataAccess;

public class OnlineDataAccessRepository : IOnlineCredentialsRepository
{
    public IMongoDatabase OnlineMongoDatabase { get; set; }
    public void GetOnlineConnection()
    {
        if (Connectivity.Current.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            IMongoDatabase db = new MongoClient("<< YOUR ATLAS CONNECTION STRING " +
                "YOU CAN FIND IT IN YOUR MONGODB ATLAS" +
                "GO TO YOUR DATABSE UNDER DEPLOYMENT" +
                "YOU'LL SEE YOUR CLUSTER (OR CREATE ONE IF INEXISTANT)" +
                "ON YOUR CLUSTER THERE WILL BE A CONNECT BUTTON, CLICK ON IT AND CLICK DRIVERS" +
                "FOR WINDOWS, USE DRIVER : C#/.NET AND VERSION 2.13 or Later!!!!>>")
                .GetDatabase("<<YOUR DB NAME>>");
            OnlineMongoDatabase = db;
        }
        else
        {
            _ = Shell.Current.DisplayAlert("No Internet Found!", "Please Connect your devcice to the internet", "OK");
        }
    }
}