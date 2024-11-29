﻿namespace FlowHub_MAUI.Utilities.TypeConverters;
public class BytesArrayToImageSource : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
        {
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();

    }
}
