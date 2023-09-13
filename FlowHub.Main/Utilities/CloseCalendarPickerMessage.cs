using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FlowHub.Main.Utilities;

public class CloseCalendarPickerMessage : ValueChangedMessage<string>
{
    public CloseCalendarPickerMessage(string value) : base(value) { }
}
