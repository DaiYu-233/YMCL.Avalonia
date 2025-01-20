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
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public sealed class Data : ReactiveObject
{
    private static Data? _instance;
    public static readonly JavaEntry AutoJava = new() { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "Auto" };
    public RunnerType RunnerType { get; set; }
    public DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.NotDesktop;
    public WindowNotificationManager Notification { get; set; }
    public ObservableCollection<Classes.MinecraftFolder> MinecraftFolders { get; set; }
    public ObservableCollection<JavaEntry> JavaRuntimes { get; set; }
    public ObservableCollection<AccountInfo> Accounts { get; set; }

    public Setting Setting { get; set; }

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}