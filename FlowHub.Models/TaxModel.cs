using System.ComponentModel;

namespace FlowHub.Models;

public class TaxModel : INotifyPropertyChanged
{
    // i need a string called Name and a double called Rate
    public string Name { get; set; }
    public double Rate { get; set; }
    bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
