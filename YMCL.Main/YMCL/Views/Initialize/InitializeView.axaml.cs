using System.Threading.Tasks;
using Irihi.Avalonia.Shared.Helpers;
using YMCL.Public.Module.App;
using YMCL.Views.Initialize.Pages;

namespace YMCL.Views.Initialize;

public partial class InitializeView : UserControl
{
    private readonly Language _language = new();
    private readonly TitleBarStyle _titleBarStyle = new();
    private readonly MinecraftFolder _minecraftFolder = new();
    private readonly JavaRuntime _javaRuntime = new();
    private readonly Account _account = new();
    private int _page = 1;

    public InitializeView()
    {
        InitializeComponent();
        BindingEvent();
    }

    public InitializeView(int page)
    {
        InitializeComponent();
        BindingEvent();
        UpdatePageAnimation(page);
    }

    public void UpdatePageAnimation(int page)
    {
        var animation = Frame.Transitions[0];
        Frame.Transitions.Clear();
        _page = page;
        Frame.Opacity = 0;
        Frame.Content = page switch
        {
            1 => _language,
            2 => _titleBarStyle,
            3 => _minecraftFolder,
            4 => _javaRuntime,
            5 => _account,
            _ => Frame.Content
        };
        Frame.Transitions.Add(animation);
        Frame.Opacity = 1;
        FinishInit(page);
    }

    private void FinishInit(int page)
    {
        if (page == 4)
        {
            Data.Setting.IsCompleteMinecraftFolderInitialize = true;
            Method.SaveSetting();
        }
        if (page == 5)
        {
            Data.Setting.IsCompleteJavaInitialize = true;
            Method.SaveSetting();
        }
        if (page == 6)
        {
            Data.Setting.IsCompleteAccountInitialize = true;
            Method.SaveSetting();
            Method.RestartApp();
        }
    }

    private void BindingEvent()
    {
        Next.Click += (_, _) => { UpdatePageAnimation(int.Min(_page + 1, 6)); };
        Precious.Click += (_, _) => { UpdatePageAnimation(int.Max(_page - 1, 1)); };
    }
}