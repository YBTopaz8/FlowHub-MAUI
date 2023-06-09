
namespace FlowHub.DataAccess.IRepositories;

public interface ISettingsServiceRepository
{
    Task<T> GetPreference<T>(string key, T defaultValue);
    Task SetPreference<T>(string key, T value);

    Task ClearPreferences();
}
