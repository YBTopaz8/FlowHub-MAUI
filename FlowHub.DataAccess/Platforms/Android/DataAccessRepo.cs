namespace FlowHub.DataAccess;

// All the code in this file is only included on Android.
public class DataAccessRepo : IDataAccessRepo
{
    LiteDatabaseAsync db ;

    public LiteDatabaseAsync GetDb() //this function returns the path where the db file is saved
    {
        string path;

        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SavingTracker.db");
        // string path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
        db = new LiteDatabaseAsync(path);
        return db;
    }
}