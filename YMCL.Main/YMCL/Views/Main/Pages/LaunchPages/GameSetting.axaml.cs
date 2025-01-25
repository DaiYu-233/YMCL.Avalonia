using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Game;
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
    private readonly SubPages.ResourcePack _resourcePack;
    private readonly SubPages.ShaderPack _shaderPack;
    private readonly SubPages.Save _save;
    private readonly SubPages.Screenshot _screenshot;

    public GameSetting(GameEntry? entry = null)
    {
        InitializeComponent();
        if (entry != null)
        {
            Model = new GameSettingModel(entry);
        }
        else
        {
            var resolver = new GameResolver(Data.Setting.MinecraftFolder.Path);
            Model = new GameSettingModel(resolver.GetGameEntity(Data.Setting.SelectedGame.Id));
        }
        DataContext = Model;
        _setting = new SubPages.Setting(Model);
        _mod = new SubPages.Mod(Model.GameEntry);
        _overView = new SubPages.OverView(Model);
        _resourcePack = new SubPages.ResourcePack(Model.GameEntry);
        _save = new SubPages.Save(Model.GameEntry);
        _shaderPack = new SubPages.ShaderPack(Model.GameEntry);
        _screenshot = new SubPages.Screenshot(Model.GameEntry);
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
                "saves" => _save,
                "resourcePacks" => _resourcePack,
                "screenshots" => _screenshot,
                "shaderPack" => _shaderPack,
                _ => FrameView.Content as UserControl
            };
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
    }
}