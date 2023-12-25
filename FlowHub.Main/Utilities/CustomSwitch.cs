using System.Windows.Input;

namespace FlowHub.Main.Utilities;

public class CustomSwitch : Microsoft.Maui.Controls.Switch
{
    public static readonly BindableProperty OnToggledCommandProperty = BindableProperty.Create(nameof(OnToggledCommand), typeof(ICommand), typeof(CustomSwitch));
    public static readonly BindableProperty OnToggledCommandParameterProperty = BindableProperty.Create(nameof(OnToggledCommandParameter), typeof(object), typeof(CustomSwitch));

    public ICommand OnToggledCommand
    {
        get => (ICommand)GetValue(OnToggledCommandProperty);
        set => SetValue(OnToggledCommandProperty, value);
    }

    public object OnToggledCommandParameter
    {
        get => GetValue(OnToggledCommandParameterProperty);
        set => SetValue(OnToggledCommandParameterProperty, value);
    }

    public CustomSwitch()
    {
        Toggled += OnToggled;
    }

    private void OnToggled(object sender, ToggledEventArgs e)
    {
        if (OnToggledCommand != null && OnToggledCommand.CanExecute(OnToggledCommandParameter))
        {
            OnToggledCommand.Execute(OnToggledCommandParameter);
        }
    }
}
