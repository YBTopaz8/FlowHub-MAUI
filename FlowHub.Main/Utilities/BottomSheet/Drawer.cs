using FlowHub.Main.Utilities.BottomSheet.Drawers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities.BottomSheet;

public class Drawer
{
    public static Task<T> Open<T>(DrawerView view) where T : new()
    {
        return PopUpSheet.Open<T>(new BaseDrawer(view));
    }

    public static Task<string> Open(DrawerView view)
    {
        return PopUpSheet.Open(new BaseDrawer(view));
    }

    public static Task Close(object returnValue = null)
    {
        return PopUpSheet.Close(returnValue);
    }
}

public class DrawerView : ContentView
{
    public Command CallBackReturn;

    public DrawerView()
    {
    }
}
