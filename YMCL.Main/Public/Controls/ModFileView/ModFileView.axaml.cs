using Avalonia.Controls;

namespace YMCL.Main.Public.Controls;

public partial class ModFileView : UserControl
{
    public ModFileView(string header)
    {
        InitializeComponent();
        Expander.Header = header;
        
    }
}