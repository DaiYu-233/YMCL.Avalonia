using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MinecraftLaunch.Classes.Models.Install;
using ReactiveUI;
using YMCL.Public.Langs;
using YMCL.Public.Module.Init.SubModule.GetDataFromNetwork;
using YMCL.ViewModels;
using Dispatcher = YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher.Dispatcher;
using String = System.String;

namespace YMCL.Views.Main.Pages.DownloadPages;

public sealed partial class InstallPreview : UserControl
{
    public readonly InstallPreviewModel Model;

    public InstallPreview(Action action, string id)
    {
        InitializeComponent();
        InstallPreviewModLoaders.Load(this, id);
        Model = new InstallPreviewModel(this);
        DataContext = Model;
        ReturnToListRoot.PointerPressed += (_, _) => action();
        InstallPreviewIdText.Text = id;
        MinecraftPreviewGameId.Text = id;
        Model.CustomId = id;
        Model.GameId = id;
        CustomIdWarning.IsVisible = false;
        FabricListView.SelectionChanged += OnSelectionChanged;
        OptiFineListView.SelectionChanged += OnSelectionChanged;
        ForgeListView.SelectionChanged += OnSelectionChanged;
        QuiltListView.SelectionChanged += OnSelectionChanged;
        ViewUpdatedContentBtn.Click += (_, _) =>
        {
            //$"https://minecraft.wiki/w/Java_Edition_{id}"
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchUriAsync(new Uri($"https://minecraft.wiki/w/Java_Edition_{id}"));
        };
        BeginInstallBtn.Click += (_, _) =>
        {
            _ = Dispatcher.Install(Model.GameId, Model.IsDisplaceId ? Model.DisplaceId : Model.CustomId, ForgeListView.SelectedItem as ForgeInstallEntry,
                FabricListView.SelectedItem as FabricBuildEntry, QuiltListView.SelectedItem as QuiltBuildEntry, OptiFineListView.SelectedItem as OptiFineInstallEntity);
        };
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        if (e.AddedItems[0] is OptiFineInstallEntity)
        {
            QuiltListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is ForgeInstallEntry)
        {
            QuiltListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is QuiltBuildEntry)
        {
            ForgeListView.SelectedItem = null;
            OptiFineListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is FabricBuildEntry)
        {
            ForgeListView.SelectedItem = null;
            OptiFineListView.SelectedItem = null;
            QuiltListView.SelectedItem = null;
        }

        Model.HandleCustomId();
        Model.UpdateSelection();
    }
}