namespace FlowHub_MAUI.Utilities.TypeConverters;

public class CollectionSizeToVisibility : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        var val = (int)value;
#pragma warning restore CS8605 // Unboxing a possibly null value.
        if (val < 1)
        {
            return false;
        }
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
