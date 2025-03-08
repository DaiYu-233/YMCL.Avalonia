using System.IO;
using System.Threading.Tasks;
using Irihi.Avalonia.Shared.Helpers;
using Newtonsoft.Json;
using YMCL.Public.Enum;
using YMCL.Public.Module;
using YMCL.Public.Module.App;
using YMCL.Views.Initialize.Pages;

namespace YMCL.Views.Initialize;

public partial class InitializeView : UserControl
{
    private readonly Language _language = new();
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
            2 => _minecraftFolder,
            3 => _javaRuntime,
            4 => _account,
            _ => Frame.Content
        };
        if (page == 1 && Data.SettingEntry.Language == new Public.Classes.Data.Language())
            Data.SettingEntry.Language = Data.Langs[0];
        Frame.Transitions.Add(animation);
        Frame.Opacity = 1;
        FinishInit(page);
    }

    private void FinishInit(int page)
    {
        if (page == 2 && Data.SettingEntry.Language.Code == null)
        {
            Data.SettingEntry.Language = Data.Langs[0];
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
        }
        if (page == 3)
        {
            Data.SettingEntry.IsCompleteMinecraftFolderInitialize = true;
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
        }
        if (page == 4)
        {
            Data.SettingEntry.IsCompleteJavaInitialize = true;
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
        }
        if (page == 5)
        {
            Data.SettingEntry.IsCompleteAccountInitialize = true;
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(Data.SettingEntry, Formatting.Indented));
            AppMethod.RestartApp();
        }
    }

    private void BindingEvent()
    {
        Next.Click += (_, _) => { UpdatePageAnimation(int.Min(_page + 1, 5)); };
        Precious.Click += (_, _) => { UpdatePageAnimation(int.Max(_page - 1, 1)); };
    }
}