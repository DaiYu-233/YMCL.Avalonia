using System.Linq;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.App.Init.SubModule;

public class SettingChanged
{
    public static void Binding()
    {
        Data.SettingEntry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingEntry.Language))
            {
                Data.JavaRuntimes.FirstOrDefault(java => java.JavaStringVersion == "Auto").JavaPath =
                    MainLang.LetYMCLChooseJava;
            }
            
            if (e.PropertyName == nameof(SettingEntry.NoticeWay))
            {
                Notice(MainLang.ExampleNotification);
            }
        };
    }
}