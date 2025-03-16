using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
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

    public GameSetting(MinecraftEntry? entry = null)
    {
        InitializeComponent();
        if (entry != null)
        {
            Model = new GameSettingModel(entry);
        }
        else
        {
            var resolver = new MinecraftParser(Data.SettingEntry.MinecraftFolder.Path);
            Model = new GameSettingModel(resolver.GetMinecraft(Data.UiProperty.SelectedMinecraft.Id));
        }

        DataContext = Model;
        _setting = new SubPages.Setting(Model);
        _mod = new SubPages.Mod(Model.MinecraftEntry);
        _overView = new SubPages.OverView(Model);
        _resourcePack = new SubPages.ResourcePack(Model.MinecraftEntry);
        _save = new SubPages.Save(Model.MinecraftEntry);
        _shaderPack = new SubPages.ShaderPack(Model.MinecraftEntry);
        _screenshot = new SubPages.Screenshot(Model.MinecraftEntry);
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            if (tag == "return")
            {
                _ = YMCL.App.UiRoot.ViewModel.Launch.CloseGameSetting();
                return;
            }

            UserControl page = tag switch
            {
                "overView" => _overView,
                "setting" => _setting,
                "mod" => _mod,
                "saves" => _save,
                "resourcePacks" => _resourcePack,
                "screenshots" => _screenshot,
                "shaderPack" => _shaderPack,
                _ => null
            };
            if (page == null) return;
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
    }

    public GameSetting()
    {
    }
}