using YMCL.Public.Enum;

namespace YMCL.Public.Module.App;

public class Decision
{
    public static (bool ifShow, int page) WhetherToShowInitView()
    {
        if (Data.Instance.Setting.Language == "Unset")
        {
            return (true, 1);
        }

        if (Data.Instance.Setting.WindowTitleBarStyle == Setting.WindowTitleBarStyle.Unset)
        {
            return (true, 2);
        }

        if (!Data.Instance.Setting.IsCompleteMinecraftFolderInitialize)
        {
            return (true, 3);
        }

        if (!Data.Instance.Setting.IsCompleteJavaInitialize)
        {
            return (true, 4);
        }

        if (!Data.Instance.Setting.IsCompleteAccountInitialize)
        {
            return (true, 5);
        } 
        
        return (false, 0);
    }
}