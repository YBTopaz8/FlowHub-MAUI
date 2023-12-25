using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities;

public class CustomSwitchHandler : SwitchHandler
{
    public static new PropertyMapper<CustomSwitch, CustomSwitchHandler> CustomSwitchMapper = new PropertyMapper<CustomSwitch, CustomSwitchHandler>(SwitchHandler.Mapper)
    {
        ["CompletionStatusLabel"] = MapCustomLabel
    };

    public CustomSwitchHandler() : base(CustomSwitchMapper)
    {
        
    }

    private static void MapCustomLabel(CustomSwitchHandler handler, CustomSwitch customSwitch)
    {

        handler.PlatformView.OffContent = "Not Completed";
        handler.PlatformView.OnContent = "Completed";
    }

    
}
