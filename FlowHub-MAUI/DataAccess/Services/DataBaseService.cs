namespace FlowHub_MAUI.DataAccess.Services;

public class DataBaseService : IDataBaseService
{
    public void DeleteDB() => throw new NotImplementedException();

    public RealmConfiguration GetRealm()
    {
        string dbPath=string.Empty;
#if ANDROID
        dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FlowHub";
#elif WINDOWS 
        dbPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FlowHub";
#endif

        if (!Directory.Exists(dbPath))
        {
            Directory.CreateDirectory(dbPath);
        }

        string filePath = Path.Combine(dbPath, "FlowHub.realm");
        //File.Delete(filePath);
        var config = new RealmConfiguration(filePath)
        {
            SchemaVersion = 0
        };

        return config;
    }

}
