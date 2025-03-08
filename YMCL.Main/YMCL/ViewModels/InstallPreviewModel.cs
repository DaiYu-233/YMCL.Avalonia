using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinecraftLaunch.Base.Models.Network;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Views.Main.Pages.DownloadPages;
using InstallPreview = YMCL.Views.Main.Pages.DownloadPages.AutoInstallPages.InstallPreview;

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
    [Reactive] public bool NeoForgeLoading { get; set; } = true;
    [Reactive] public bool QuiltLoading { get; set; } = true;
    [Reactive] public bool FabricLoading { get; set; } = true;
    [Reactive] public bool NoOptiFine { get; set; }
    [Reactive] public bool NoForge { get; set; }
    [Reactive] public bool NoQuilt { get; set; }
    [Reactive] public bool NoFabric { get; set; }
    [Reactive] public bool NoNeoForge { get; set; }
    [Reactive] public string SelectedOptiFine { get; set; }
    [Reactive] public string SelectedForge { get; set; }
    [Reactive] public string SelectedNeoForge { get; set; }
    [Reactive] public string SelectedQuilt { get; set; }
    [Reactive] public string SelectedFabric { get; set; }
    [Reactive] public ObservableCollection<OptifineInstallEntry> OptiFines { get; set; } = [];
    [Reactive] public ObservableCollection<ForgeInstallEntry> Forges { get; set; } = [];
    [Reactive] public ObservableCollection<ForgeInstallEntry> NeoForges { get; set; } = [];
    [Reactive] public ObservableCollection<QuiltInstallEntry> Quilts { get; set; } = [];
    [Reactive] public ObservableCollection<FabricInstallEntry> Fabrics { get; set; } = [];

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
                entry.NeoForgeListView.SelectedIndex >= 0 ||
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
            var optifine = (OptifineInstallEntry)entry.OptiFineListView.SelectedItem;
            return $"{GameId}-Forge_{forge.ForgeVersion}-Optifine_{optifine.Type}_{optifine.Patch}";
        }

        if (entry.ForgeListView.SelectedIndex >= 0)
        {
            var forge = (ForgeInstallEntry)entry.ForgeListView.SelectedItem;
            return $"{GameId}-Forge {forge.ForgeVersion}";
        }

        if (entry.NeoForgeListView.SelectedIndex >= 0)
        {
            var neoforge = (ForgeInstallEntry)entry.NeoForgeListView.SelectedItem;
            return $"{GameId}-NeoForge {neoforge.ForgeVersion}";
        }

        if (entry.OptiFineListView.SelectedIndex >= 0)
        {
            var optifine = (OptifineInstallEntry)entry.OptiFineListView.SelectedItem;
            return $"{GameId}-Optifine {optifine.Type} {optifine.Patch}";
        }

        if (entry.FabricListView.SelectedIndex >= 0)
        {
            var fabric = (FabricInstallEntry)entry.FabricListView.SelectedItem;
            return $"{GameId}-Fabric {fabric.BuildVersion}";
        }

        if (entry.QuiltListView.SelectedIndex < 0) return string.Empty;
        var quilt = (QuiltInstallEntry)entry.QuiltListView.SelectedItem;
        return $"{GameId}-Quilt {quilt.BuildVersion}";
    }

    public void UpdateSelection()
    {
        SelectedFabric = entry.FabricListView.SelectedItem as FabricInstallEntry != null
            ? (entry.FabricListView.SelectedItem as FabricInstallEntry).BuildVersion
            : "Null";
        SelectedQuilt = entry.QuiltListView.SelectedItem as QuiltInstallEntry != null
            ? (entry.QuiltListView.SelectedItem as QuiltInstallEntry).BuildVersion
            : "Null";
        SelectedForge = entry.ForgeListView.SelectedItem as ForgeInstallEntry != null
            ? (entry.ForgeListView.SelectedItem as ForgeInstallEntry).ForgeVersion
            : "Null";
        SelectedNeoForge = entry.NeoForgeListView.SelectedItem as ForgeInstallEntry != null
            ? (entry.NeoForgeListView.SelectedItem as ForgeInstallEntry).ForgeVersion
            : "Null";
        SelectedOptiFine = entry.OptiFineListView.SelectedItem as OptifineInstallEntry != null
            ? $"{(entry.OptiFineListView.SelectedItem as OptifineInstallEntry).Type} {(entry.OptiFineListView.SelectedItem as OptifineInstallEntry).Patch}"
            : "Null";
    }
}