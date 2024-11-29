namespace FlowHub_MAUI.Utilities.TypeConverters;
public class BytesToMegabytesConverter : IValueConverter
{

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        if (value is long)
        {
            return ((long)value / 1024.0 / 1024.0).ToString("0.##") + " MB";
        }
        else if (value is double)
        {
            return ((double)value / 1024.0 / 1024.0).ToString("0.##") + " MB";
        }
        return "0 MB"; // Default case if conversion fails
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
