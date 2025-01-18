using YMCL.Public.Enum;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public class Data
{
    public static RunnerType RunnerType { get; set; }
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.NotDesktop;
    public static Setting Setting { get; set; }
}