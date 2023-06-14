using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Models;

public class TaxModel
{
    // i need a string called Name and a double called Rate
    public string Name { get; set; }
    public double Rate { get; set; }
    public bool IsChecked { get; set; }
}
