using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinecraftLaunch.Classes.Models.Install;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Views.Main.Pages.DownloadPages;

namespace YMCL.ViewModels;

public sealed class InstallPreviewModel : ReactiveObject
{
    private readonly InstallPreview entry;

    public InstallPreviewModel(InstallPreview entry)
    {
        this.entry = entry;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CustomId))
            {
                HandleCustomId();
            }
        };
        UpdateSelection();
    }

    [Reactive] public string GameId { get; set; }
    [Reactive] public string CustomId { get; set; }
    [Reactive] public string DisplaceId { get; set; }
    [Reactive] public bool IsDisplaceId { get; set; }
    [Reactive] public bool OptifineLoading { get; set; } = true;
    [Reactive] public bool ForgeLoading { get; set; } = true;
    [Reactive] public bool QuiltLoading { get; set; } = true;
    [Reactive] public bool FabricLoading { get; set; } = true;
    [Reactive] public string SelectedOptiFine { get; set; }
    [Reactive] public string SelectedForge { get; set; }
    [Reactive] public string SelectedQuilt { get; set; }
    [Reactive] public string SelectedFabric { get; set; }
    [Reactive] public ObservableCollection<OptiFineInstallEntity> OptiFines { get; set; } = [];
    [Reactive] public ObservableCollection<ForgeInstallEntry> Forges { get; set; } = [];
    [Reactive] public ObservableCollection<QuiltBuildEntry> Quilts { get; set; } = [];
    [Reactive] public ObservableCollection<FabricBuildEntry> Fabrics { get; set; } = [];

    public void HandleCustomId()
    {
        if (!ShouldUpdateCustomId())
        {
            IsDisplaceId = false;
            return;
        }

        entry.CustomIdWarning.IsVisible = true;
        DisplaceId = GenerateNewId();
        IsDisplaceId = true;
    }

    private bool ShouldUpdateCustomId()
    {
        return (entry.ForgeListView.SelectedIndex >= 0 ||
                entry.FabricListView.SelectedIndex >= 0 ||
                entry.QuiltListView.SelectedIndex >= 0 ||
                entry.OptiFineListView.SelectedIndex >= 0) &&
               CustomId == GameId;
    }

    private string GenerateNewId()
    {
        if (entry.ForgeListView.SelectedIndex >= 0 && entry.OptiFineListView.SelectedIndex >= 0)
        {
            var forge = (ForgeInstallEntry)entry.ForgeListView.SelectedItem;
            var optifine = (OptiFineInstallEntity)entry.OptiFineListView.SelectedItem;
            return $"{GameId}-Forge_{forge.ForgeVersion}-Optifine_{optifine.Type}_{optifine.Patch}";
        }

        if (entry.ForgeListView.SelectedIndex >= 0)
        {
            var forge = (ForgeInstallEntry)entry.ForgeListView.SelectedItem;
            return $"{GameId}-Forge_{forge.ForgeVersion}";
        }

        if (entry.OptiFineListView.SelectedIndex >= 0)
        {
            var optifine = (OptiFineInstallEntity)entry.OptiFineListView.SelectedItem;
            return $"{GameId}-Optifine_{optifine.Type}_{optifine.Patch}";
        }

        if (entry.FabricListView.SelectedIndex >= 0)
        {
            var fabric = (FabricBuildEntry)entry.FabricListView.SelectedItem;
            return $"{GameId}-Fabric {fabric.BuildVersion}";
        }

        if (entry.QuiltListView.SelectedIndex < 0) return string.Empty;
        var quilt = (QuiltBuildEntry)entry.QuiltListView.SelectedItem;
        return $"{GameId}-Quilt {quilt.BuildVersion}";
    }

    public void UpdateSelection()
    {
        SelectedFabric = entry.FabricListView.SelectedItem as FabricBuildEntry != null
            ? (entry.FabricListView.SelectedItem as FabricBuildEntry).BuildVersion
            : "Null";
        SelectedQuilt = entry.QuiltListView.SelectedItem as QuiltBuildEntry != null
            ? (entry.QuiltListView.SelectedItem as QuiltBuildEntry).BuildVersion
            : "Null";
        SelectedForge = entry.ForgeListView.SelectedItem as ForgeInstallEntry != null
            ? (entry.ForgeListView.SelectedItem as ForgeInstallEntry).ForgeVersion
            : "Null";
        SelectedOptiFine = entry.OptiFineListView.SelectedItem as OptiFineInstallEntity != null
            ? $"{(entry.OptiFineListView.SelectedItem as OptiFineInstallEntity).Type} {(entry.OptiFineListView.SelectedItem as OptiFineInstallEntity).Patch}"
            : "Null";
    }
}