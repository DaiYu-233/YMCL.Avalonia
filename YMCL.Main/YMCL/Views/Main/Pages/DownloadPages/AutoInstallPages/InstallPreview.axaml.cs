using MinecraftLaunch.Base.Models.Network;
using YMCL.Public.Module.App.Init.SubModule.GetDataFromNetwork;
using YMCL.ViewModels;
using Dispatcher = YMCL.Public.Module.Mc.Installer.Minecraft.Dispatcher;

namespace YMCL.Views.Main.Pages.DownloadPages.AutoInstallPages;

public sealed partial class InstallPreview : UserControl
{
    public readonly InstallPreviewModel Model;

    public InstallPreview(Action action, VersionManifestEntry entry)
    {
        InitializeComponent();
        InstallPreviewModLoaders.Load(this, entry.Id);
        Model = new InstallPreviewModel(this);
        DataContext = Model;
        ReturnToListRoot.PointerPressed += (_, _) => action();
        InstallPreviewIdText.Text = entry.Id;
        MinecraftPreviewGameId.Text = entry.Id;
        Model.CustomId = entry.Id;
        Model.GameId = entry.Id;
        CustomIdWarning.IsVisible = false;
        FabricListView.SelectionChanged += OnSelectionChanged;
        OptiFineListView.SelectionChanged += OnSelectionChanged;
        ForgeListView.SelectionChanged += OnSelectionChanged;
        NeoForgeListView.SelectionChanged += OnSelectionChanged;
        QuiltListView.SelectionChanged += OnSelectionChanged;
        ViewUpdatedContentBtn.Click += (_, _) =>
        {
            //$"https://minecraft.wiki/w/Java_Edition_{id}"
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchUriAsync(new Uri($"https://minecraft.wiki/w/Java_Edition_{entry.Id}"));
        };
        BeginInstallBtn.Click += (_, _) =>
        {
            _ = Dispatcher.Install(entry, Model.IsDisplaceId ? Model.DisplaceId : Model.CustomId,
                ForgeListView.SelectedItem as ForgeInstallEntry, NeoForgeListView.SelectedItem as ForgeInstallEntry,
                FabricListView.SelectedItem as FabricInstallEntry, QuiltListView.SelectedItem as QuiltInstallEntry,
                OptiFineListView.SelectedItem as OptifineInstallEntry);
        };
    }

    public InstallPreview()
    {
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        if (e.AddedItems[0] is OptifineInstallEntry)
        {
            QuiltListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
            NeoForgeListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is ForgeInstallEntry { IsNeoforge: false })
        {
            QuiltListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
            NeoForgeListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is QuiltInstallEntry)
        {
            ForgeListView.SelectedItem = null;
            OptiFineListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
            NeoForgeListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is FabricInstallEntry)
        {
            ForgeListView.SelectedItem = null;
            OptiFineListView.SelectedItem = null;
            QuiltListView.SelectedItem = null;
            NeoForgeListView.SelectedItem = null;
        }
        else if (e.AddedItems[0] is ForgeInstallEntry { IsNeoforge: true })
        {
            QuiltListView.SelectedItem = null;
            FabricListView.SelectedItem = null;
            ForgeListView.SelectedItem = null;
            OptiFineListView.SelectedItem = null;
            QuiltListView.SelectedItem = null;
        }

        Model.HandleCustomId();
        Model.UpdateSelection();
    }
}