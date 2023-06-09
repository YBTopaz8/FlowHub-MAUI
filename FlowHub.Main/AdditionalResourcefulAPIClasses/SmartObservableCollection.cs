
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FlowHub.Main.AdditionalResourcefulAPIClasses;

public class SmartObservableCollection<T> : ObservableCollection<T>
{
	public void AddRange (IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			Items.Add(item);
		}
		this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
		this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public void Reset(IEnumerable<T> range)
	{
		Items.Clear();

		AddRange(range);
	}
}
