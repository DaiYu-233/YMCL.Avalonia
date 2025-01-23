using System.Linq;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.App.SubModule.NetAndUiLoader;

public class InstallPreviewModLoaders
{
    public static void Load(YMCL.Views.Main.Pages.DownloadPages.InstallPreview view, string id)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await OptifineInstaller.EnumerableFromVersionAsync(id)).ToList();
                view.Model.OptifineLoading = false;
                list.ForEach(item => { view.Model.OptiFines.Add(item); });
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.GetFail}: OptiFine", ex);
                    view.Model.OptifineLoading = false;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await ForgeInstaller.EnumerableFromVersionAsync(id)).ToList();
                view.Model.ForgeLoading = false;
                list.ForEach(item => { view.Model.Forges.Add(item); });
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.GetFail}: Forge", ex);
                    view.Model.ForgeLoading = false;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await QuiltInstaller.EnumerableFromVersionAsync(id)).ToList();
                view.Model.QuiltLoading = false;
                list.ForEach(item => { view.Model.Quilts.Add(item); });
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.GetFail}: Quilt", ex);
                    view.Model.QuiltLoading = false;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await FabricInstaller.EnumerableFromVersionAsync(id)).ToList();
                view.Model.FabricLoading = false;
                list.ForEach(item => { view.Model.Fabrics.Add(item); });
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.GetFail}: Fabric", ex);
                    view.Model.FabricLoading = false;
                });
            }
        });
    }
}