using FlowHub.DataAccess.IRepositories;
using LiteDB.Async;

namespace FlowHub.DataAccess;

// All the code in this file is only included on Windows.

public class DataAccessRepo : IDataAccessRepo
{
    LiteDatabaseAsync db = null;
    public LiteDatabaseAsync GetDb() //this function returns the path where the db file is saved
    {
        string path;

        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SavingsTracker\\");
        bool DirectoryExists = Directory.Exists(path);
        if (!DirectoryExists)
        {
            var fileName = "SavingTracker.db";
            Directory.CreateDirectory(path);

            db = new LiteDatabaseAsync(path + fileName);
        }
        else
        {
            var fileName = "SavingTracker.db";
            string connectionString = $"Filename={path}{fileName};Connection=shared";
            db = new LiteDatabaseAsync(connectionString);
        }
        return db;
    }
}