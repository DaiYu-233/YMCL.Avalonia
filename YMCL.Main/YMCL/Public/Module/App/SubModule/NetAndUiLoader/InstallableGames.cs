using System.Linq;
using MinecraftLaunch;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.App.SubModule.NetAndUiLoader;

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

            UiProperty.Instance.InstallableRingIsVisible = false;
        });
    }
}