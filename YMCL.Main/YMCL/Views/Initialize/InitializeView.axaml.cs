using System.Threading.Tasks;
using Irihi.Avalonia.Shared.Helpers;
using YMCL.Public.Enum;
using YMCL.Public.Module;
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
        if (page == 1 && Data.Setting.Language == new Public.Classes.Language())
            Data.Setting.Language = Data.Langs[0];
        Frame.Transitions.Add(animation);
        Frame.Opacity = 1;
        FinishInit(page);
    }

    private void FinishInit(int page)
    {
        if (page == 2 && Data.Setting.Language.Code == null)
        {
            Data.Setting.Language = Data.Langs[0];
        }
        if (page == 3 && Data.Setting.WindowTitleBarStyle == Setting.WindowTitleBarStyle.Unset)
        {
            Data.Setting.WindowTitleBarStyle = Setting.WindowTitleBarStyle.System;
        }
        if (page == 4)
        {
            Data.Setting.IsCompleteMinecraftFolderInitialize = true;
        }
        if (page == 5)
        {
            Data.Setting.IsCompleteJavaInitialize = true;
        }
        if (page == 6)
        {
            Data.Setting.IsCompleteAccountInitialize = true;
            Method.RestartApp();
        }
    }

    private void BindingEvent()
    {
        Next.Click += (_, _) => { UpdatePageAnimation(int.Min(_page + 1, 6)); };
        Precious.Click += (_, _) => { UpdatePageAnimation(int.Max(_page - 1, 1)); };
    }
}