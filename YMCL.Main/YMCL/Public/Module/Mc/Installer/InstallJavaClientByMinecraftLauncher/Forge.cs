using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Resolver;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher;

public class Forge
{
    public static async Task<bool> Install(string id, string customId, ForgeInstallEntry entry, SubTask subTask,
        TaskEntry task,CancellationToken cancellationToken = default)
    {
        var shouldReturn = false;
        await Task.Run(async () =>
        {
            try
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    subTask.State = TaskState.Running;
                });
                if (Data.JavaRuntimes.Count <= 0)
                {
                    Toast(MainLang.CannotFandRightJava, NotificationType.Error);
                    shouldReturn = true;
                }
                else
                {
                    var resolver = new GameResolver(Data.Setting.MinecraftFolder.Path);
                    var game = resolver.GetGameEntity(id);
                    var forgeInstaller = new ForgeInstaller(game, entry, Data.JavaRuntimes[0].JavaPath,
                        customId,  new DownloaderConfiguration
                        {
                            MaxThread = Data.Setting.MaximumDownloadThread,
                            IsEnableFragmentedDownload = Data.Setting.DownloadSource != Setting.DownloadSource.Mojang
                        });
                    forgeInstaller.ProgressChanged += (_, x) =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateValue(x.Progress * 100);
                        });
                    };

                    var result = await forgeInstaller.InstallAsync(cancellationToken);

                    if (result)
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Toast($"{MainLang.InstallFinish}: Forge - {customId}", NotificationType.Success);
                        });
                    }
                    else
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Toast($"{MainLang.InstallFail}: Forge - {customId}", NotificationType.Error);
                        });
                        shouldReturn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.InstallFail}: Forge - {customId}", ex);
                });
                shouldReturn = true;
            }
        }); 
        return !shouldReturn;
    }
}