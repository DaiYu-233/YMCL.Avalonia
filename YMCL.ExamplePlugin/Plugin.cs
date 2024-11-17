using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin;

public class Main : IPlugin
{
    ExampleView view = new();
    public PluginInfo GetPluginInformation()
    {
        return new PluginInfo
        {
            Author = "Example Plugin",
            Name = "Example Plugin",
            Version = "1.3.0",
            Description = "A Example Plugin of YMCL.",
            Time = new DateTime(2025, 1, 1, 0, 0, 0)
        };
    }

    private readonly NavigationViewItem _item = new()
    {
        Content = "Example",
        Tag = "Example",Name = "ExampleNav"
    };
    public void Init()
    {
        var nav = Const.Window.main.FindControl<NavigationView>("Nav");
        nav.MenuItems.Add(_item);
        nav.SelectionChanged += (_, _) =>
        {
            if ((string)((NavigationViewItem)nav.SelectedItem).Tag == "Example")
            {
                Const.Window.main.FindControl<Frame>("FrameView").Content = view;
            }
        };
    }

    public void OnLoad()
    {
        Init();
    }

    public void OnEnable()
    {
        Method.Ui.RestartApp();
    }

    public void OnDisable()
    {
        Method.Ui.RestartApp();
    }
}