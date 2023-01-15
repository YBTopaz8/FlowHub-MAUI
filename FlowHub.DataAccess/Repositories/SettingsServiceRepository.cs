using FlowHub.DataAccess.IRepositories;

namespace FlowHub.DataAccess.Repositories;

public class SettingsServiceRepository : ISettingsServiceRepository
{
    public Task<T> GetPreference<T>(string key, T defaultValue)
    {
        var result = Preferences.Default.Get<T>(key, defaultValue); 
        return Task.FromResult(result);
    }
    public Task SetPreference<T>(string key, T value)
    {
        Preferences.Default.Set<T>(key, value); 
        return Task.CompletedTask;
    }
    public Task ClearPreferences()
    {
        Preferences.Default.Clear();
        return Task.CompletedTask;
    }
}
