using LiteDB.Async;

namespace FlowHub.DataAccess.IRepositories;

public interface IDataAccessRepo
{
    LiteDatabaseAsync GetDb();
}
