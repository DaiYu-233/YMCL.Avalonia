using System.Collections.Generic;
using Avalonia.VisualTree;

namespace YMCL.Public.Module.Ui;

public class Getter
{
    public static Control? FindControlByName(Visual parent, string name)
    {
        var visuals = new Queue<Visual>();
        visuals.Enqueue(parent);

        while (visuals.Count > 0)
        {
            var current = visuals.Dequeue();

            if (current is not Control control) continue;
            foreach (var child in control.GetVisualChildren())
            {
                visuals.Enqueue(child);
            }

            if (control.Name == name)
            {
                return control;
            }
        }

        return null;
    }
}