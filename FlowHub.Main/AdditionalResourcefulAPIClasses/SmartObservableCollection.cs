using System.Collections.Specialized;
using System.ComponentModel;

namespace FlowHub.Main.AdditionalResourcefulAPIClasses;

public class SmartObservableCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Items.Add(item);
        }
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reset(IEnumerable<T> range)
    {
        Items.Clear();

        AddRange(range);
    }
}
