using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Classes.Models.Game;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.ViewModels;
using YMCL.Views.Initialize.Pages;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public class Data
{
    public static RunnerType RunnerType { get; set; }
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.NotDesktop;
    public static WindowNotificationManager Notification { get; set; } 
    public static Setting Setting { get; set; }
    public static ObservableCollection<Classes.MinecraftFolder> MinecraftFolders { get; set; }
    public static ObservableCollection<JavaEntry> JavaRuntimes { get; set; }
    public static ObservableCollection<AccountInfo> Accounts { get; set; }
}