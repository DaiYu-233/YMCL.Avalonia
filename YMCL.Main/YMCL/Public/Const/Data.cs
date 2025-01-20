using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Classes.Models.Game;
using ReactiveUI;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;
using YMCL.ViewModels;
using YMCL.Views.Initialize.Pages;
using Language = YMCL.Public.Classes.Language;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public sealed class Data : ReactiveObject
{
    private static Data? _instance;
    public static RunnerType RunnerType { get; set; }
    public static ObservableCollection<Language> Langs => LangHelper.Langs;
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.NotDesktop;
    public static WindowNotificationManager Notification { get; set; }
    public static ObservableCollection<Classes.MinecraftFolder> MinecraftFolders { get; set; }
    public static ObservableCollection<JavaEntry> JavaRuntimes { get; set; }
    public static ObservableCollection<AccountInfo> Accounts { get; set; }

    public static Setting Setting { get; set; }

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }
}