namespace FlowHub.DataAccess.IRepositories;

public interface IOnlineCredentialsRepository
{
    public IMongoDatabase OnlineMongoDatabase { get; set; }
    void GetOnlineConnection();
}
