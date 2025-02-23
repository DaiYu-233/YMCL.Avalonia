using System.Linq;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Langs;
using YMCL.Views.Main.Pages.DownloadPages.AutoInstallPages;

namespace YMCL.Public.Module.Init.SubModule.GetDataFromNetwork;

public class InstallPreviewModLoaders
{
    public static void Load(InstallPreview view, string id)
    {
        _ = Task.Run(async () => // OptiFine
        {
            try
            {
                var list = await OptifineInstaller.EnumerableOptifineAsync(id).ToListAsync();
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
        _ = Task.Run(async () => // Forge
        {
            try
            {
                var list = await ForgeInstaller.EnumerableForgeAsync(id).ToListAsync();
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
        _ = Task.Run(async () => // NeoForge
        {
            try
            {
                var list = await ForgeInstaller.EnumerableForgeAsync(id, true).ToListAsync();
                view.Model.NeoForgeLoading = false;
                list.ForEach(item => { view.Model.NeoForges.Add(item); });
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowShortException($"{MainLang.GetFail}: NeoForge", ex);
                    view.Model.NeoForgeLoading = false;
                });
            }
        });
        _ = Task.Run(async () => // Quilt
        {
            try
            {
                var list = await QuiltInstaller.EnumerableQuiltAsync(id).ToListAsync();
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
        _ = Task.Run(async () => // Fabric
        {
            try
            {
                var list = await FabricInstaller.EnumerableFabricAsync(id).ToListAsync();
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