using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities;

public class PopUpCloseResult
{
    public object Data { get; set; }
    public PopupResult Result { get; set; }
}

public enum PopupResult
{
    OK,
    Cancel,
    Delete
}