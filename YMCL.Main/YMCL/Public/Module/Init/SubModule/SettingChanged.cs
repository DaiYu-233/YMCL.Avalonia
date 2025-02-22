using System.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Init.SubModule;

public class SettingChanged
{
    public static void Binding()
    {
        Data.Setting.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Setting.Language))
            {
                Data.JavaRuntimes.FirstOrDefault(java => java.JavaStringVersion == "Auto").JavaPath =
                    MainLang.LetYMCLChooseJava;
            }
            
            if (e.PropertyName == nameof(Setting.NoticeWay))
            {
                Notice(MainLang.ExampleNotification);
            }
        };
    }
}