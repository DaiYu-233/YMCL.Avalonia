using System.Linq;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;

namespace YMCL.Public.Module.Init.SubModule.NetAndUiLoader;

public class InstallableGame
{
    public static async Task Load()
    {
        await Task.Run(async () =>
        {
            var gameCoreList = await VanlliaInstaller.EnumerableGameCoreAsync();
            var manifestEntries = gameCoreList.ToList();
            UiProperty.Instance.LatestReleaseGame = manifestEntries.FirstOrDefault(item => item.Type == "release");
            UiProperty.Instance.LatestSnapshotGame = manifestEntries.FirstOrDefault(item => item.Type == "snapshot");
            foreach (var item in manifestEntries)
            {
                UiProperty.AllInstallableGames.Add(item);
                if (item.Type == "release")
                {
                    UiProperty.ReleaseInstallableGames.Add(item);
                }
                else if (item.Type == "snapshot")
                {
                    UiProperty.SnapshotInstallableGames.Add(item);
                }
                else
                {
                    UiProperty.OldInstallableGames.Add(item);
                }
            }

            Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
            UiProperty.Instance.InstallableRingIsVisible = false;
        });
        App.UiRoot.ViewModel.Download._autoInstall.InstallableGames.Filter();
    }
}