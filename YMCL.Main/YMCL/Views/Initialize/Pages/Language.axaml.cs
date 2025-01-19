using CommunityToolkit.Mvvm.ComponentModel;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Views.Initialize.Pages;

public partial class Language : UserControl
{
    public Language()
    {
        InitializeComponent();
        ControlProperty();
        EventBinding();
        DataContext = Data.Instance.Setting;
    }

    private void ControlProperty()
    {
        var lang = Data.Instance.Setting.Language;
        if (lang == "Unset") return;
        LanguageListBox.SelectedIndex = lang switch
        {
            "zh-CN" => 0,
            "zh-Hant" => 1,
            "en-US" => 2,
            "ja-JP" => 3,
            "ru-RU" => 4,
            _ => LanguageListBox.SelectedIndex
        };
    }

    private void EventBinding()
    {
        LanguageListBox.SelectionChanged += (_, _) =>
        {
            var selectedItem = (StackPanel)LanguageListBox.SelectedItem;
            var cultureCode = ((TextBlock)selectedItem.Children[1]).Text;
            LangHelper.Current.ChangedCulture(cultureCode!);
            Data.Instance.Setting.Language = cultureCode;
        };
    }
}