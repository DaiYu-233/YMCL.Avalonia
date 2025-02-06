﻿using System.Linq;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Module.Init.SubModule.GetDataFromNetwork;

public class InstallableGame
{
    public static async Task Load()
    {
        await Task.Run(async () =>
        {
            try
            {
                var gameCoreList = await VanlliaInstaller.EnumerableGameCoreAsync();
                var manifestEntries = gameCoreList.ToList();
                UiProperty.Instance.LatestReleaseGame = manifestEntries.FirstOrDefault(item => item.Type == "release");
                UiProperty.Instance.LatestSnapshotGame =
                    manifestEntries.FirstOrDefault(item => item.Type == "snapshot");
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

                AggregateSearchUi.UpdateAllAggregateSearchEntries();
                UiProperty.Instance.InstallableRingIsVisible = false;
            }
            catch
            {
                await Task.Delay(700);
                Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                {
                    App.UiRoot.ViewModel.Download._autoInstall.InstallableGames.LoadInstallableVersionListErrorInfoBar
                        .IsVisible = true;
                    UiProperty.Instance.LatestReleaseGame = new VersionManifestEntry
                        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.LoadFail, Type = null };
                    UiProperty.Instance.LatestSnapshotGame = new VersionManifestEntry
                        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.LoadFail, Type = null };
                });
            }
        });
        App.UiRoot.ViewModel.Download._autoInstall.InstallableGames.Filter();
    }
}