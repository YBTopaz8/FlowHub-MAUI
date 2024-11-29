namespace FlowHub_MAUI.Utilities.OtherUtils;

public static class GeneralStaticUtilities
{
    public static string GenerateRandomString(string CallerClass, int length = 12)
    {
        if (string.IsNullOrEmpty(CallerClass))
        {
            throw new ArgumentNullException(nameof(CallerClass));
        }

        Random random = Random.Shared;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        stringChars[0] = CallerClass[0];
        stringChars[1] = CallerClass[1];

        for (int i = 2; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}
