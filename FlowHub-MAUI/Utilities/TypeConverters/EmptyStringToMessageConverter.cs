namespace FlowHub_MAUI.Utilities.TypeConverters;
public class EmptyStringToMessageConverter : IValueConverter // TODO: RENAME THIS
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {        

       
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
