using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Components.Resolver;
using YMCL.Public.Module.Ui;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages.LaunchPages;

public partial class GameSetting : UserControl
{
    public readonly GameSettingModel Model;
    private readonly SubPages.Setting _setting;
    private readonly SubPages.Mod _mod;
    private readonly SubPages.OverView _overView;

    public GameSetting()
    {
        InitializeComponent();
        var resolver = new GameResolver(Data.Setting.MinecraftFolder.Path);
        Model = new GameSettingModel(resolver.GetGameEntity(Data.Setting.SelectedGame.Id));
        DataContext = Model;
        _setting = new SubPages.Setting(Model);
        _mod = new SubPages.Mod(Model.GameEntry);
        _overView = new SubPages.OverView(Model);
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            if (tag == "return")
            {
                _ = App.UiRoot.ViewModel.Launch.CloseGameSetting();
                return;
            }

            var page = tag switch
            {
                "overView" => _overView,
                "setting" => _setting,
                "mod" => _mod,
                _ => FrameView.Content as UserControl
            };
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
    }
}