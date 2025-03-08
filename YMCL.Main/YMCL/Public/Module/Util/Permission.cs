using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Public.Module.Util;

public class Permission
{
    public static bool IsAdministrator()
    {
        if (Const.Data.DesktopType != DesktopRunnerType.Windows) return false;
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    public static async Task<bool> TryToUpgradePermission()
    {
        if (Const.Data.DesktopType != DesktopRunnerType.Windows) return true;
        if (IsAdministrator()) return true;
        var cr = await ShowDialogAsync(MainLang.UpgradeToAdministratorPrivileges,
            MainLang.UpgradeToAdministratorPrivilegesTip, b_primary: MainLang.Ok, b_cancel: MainLang.Cancel);
        if (cr == ContentDialogResult.Primary)
        {
            AppMethod.RestartApp(true);
            return true;
        }
        else
        {
            return false;
        }
    }
}