using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using YMCL.Public.Enum;
using YMCL.ViewModels;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public class Data
{
    public static RunnerType RunnerType { get; set; }
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.NotDesktop;
    public static Setting Setting { get; set; }
    public static ObservableCollection<Classes.MinecraftFolder> MinecraftFolders { get; set; } 
}