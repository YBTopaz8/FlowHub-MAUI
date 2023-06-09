
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.DataAccess.IRepositories;

public interface IOnlineCredentialsRepository
{
    public IMongoDatabase OnlineMongoDatabase { get; set; }
    void GetOnlineConnection();
}
