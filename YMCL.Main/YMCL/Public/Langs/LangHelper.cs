using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace YMCL.Public.Langs;

public class LangHelper : INotifyPropertyChanged
{
    public static LangHelper Current { get; } = new();

    public MainLang Resources { get; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ChangedCulture(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "Unset")
            MainLang.Culture = CultureInfo.GetCultureInfo("zh-CN");
        else
            MainLang.Culture = CultureInfo.GetCultureInfo(name);
        RaisePropertyChanged(nameof(Resources));
    }

    public static string GetText(string name, string culture = "")
    {
        var setting = Const.Data.Setting;
        var cultureInfo = culture == "" ? CultureInfo.GetCultureInfo(setting.Language is "zh-CN" or null ? "" : setting.Language) : CultureInfo.GetCultureInfo(culture);

        var res = MainLang.ResourceManager.GetObject(name, cultureInfo).ToString();
        return res ?? "Null";
    }
}