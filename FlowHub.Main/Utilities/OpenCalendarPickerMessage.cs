using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FlowHub.Main.Utilities;

public class OpenCalendarPickerMessage : ValueChangedMessage<string>
{
    public OpenCalendarPickerMessage(string value) : base(value) { }
}
