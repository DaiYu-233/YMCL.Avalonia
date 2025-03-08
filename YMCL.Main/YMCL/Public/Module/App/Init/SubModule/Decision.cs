namespace YMCL.Public.Module.App.Init.SubModule;

public class Decision
{
    public static (bool ifShow, int page) WhetherToShowInitView()
    {
        if (Data.SettingEntry.Language.Code == null)
        {
            return (true, 1);
        }

        if (!Data.SettingEntry.IsCompleteMinecraftFolderInitialize)
        {
            return (true, 2);
        }

        if (!Data.SettingEntry.IsCompleteJavaInitialize)
        {
            return (true, 3);
        }

        if (!Data.SettingEntry.IsCompleteAccountInitialize)
        {
            return (true, 4);
        } 
        
        return (false, 0);
    }
}