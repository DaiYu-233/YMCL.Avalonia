using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MinecraftLaunch.Base.Models.Network;
using YMCL.Public.Module.Ui;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages.DownloadPages;

public partial class AutoInstall : UserControl
{
    public readonly AutoInstallPages.InstallableGames InstallableGames = new();

    public AutoInstall()
    {
        InitializeComponent();
        Frame.Content = InstallableGames;
    }

    public void JumpToInstallPreview(VersionManifestEntry entry)
    {
        var page = new AutoInstallPages.InstallPreview(() =>
        {
            Frame.Content = InstallableGames;
            _ = Animator.PageLoading.LevelTwoPage(InstallableGames);
        }, entry);
        Frame.Content = page;
        _ = Animator.PageLoading.LevelTwoPage(page);
    }
}