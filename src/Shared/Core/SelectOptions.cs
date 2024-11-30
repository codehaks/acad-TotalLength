using System.Collections.Generic;

namespace MyApp.Core;

public class SelectOptions
{
    public bool SelectLines { get; set; }
    public bool SelectPolyLines { get; set; }
    public bool SelectArcs { get; set; }

    public override string ToString()
    {
        var selectedTypes = new List<string>();

        if (SelectLines)
            selectedTypes.Add("LINE");
        if (SelectPolyLines)
            selectedTypes.Add("LWPOLYLINE");
        if (SelectArcs)
            selectedTypes.Add("ARC");

        return string.Join(",", selectedTypes);
    }
}