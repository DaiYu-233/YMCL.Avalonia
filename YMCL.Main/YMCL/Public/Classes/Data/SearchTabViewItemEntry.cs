using System.Linq;

namespace YMCL.Public.Classes.Data;

public record SearchTabViewItemEntry
{
    public UserControl Content { get; set; }
    public string Title { get; set; }
    public string Tag { get; set; }
    public string Host { get; set; }
    public bool CanClose { get; set; }

    public void Close()
    {
        if (Host == nameof(Views.Main.Pages.DownloadPages.CurseForge))
        {
            if (YMCL.App.UiRoot.ViewModel.Download._curseForge.SelectedItem == this)
            {
                YMCL.App.UiRoot.ViewModel.Download._curseForge.Items.Remove(this);

                YMCL.App.UiRoot.ViewModel.Download._curseForge.SelectedItem =
                    YMCL.App.UiRoot.ViewModel.Download._curseForge.Items.LastOrDefault();
            }
            else
            {
                YMCL.App.UiRoot.ViewModel.Download._curseForge.Items.Remove(this);
            }
        }
        else if (Host == nameof(Views.Main.Pages.DownloadPages.Modrinth))
        {
            if (YMCL.App.UiRoot.ViewModel.Download._modrinth.SelectedItem == this)
            {
                YMCL.App.UiRoot.ViewModel.Download._modrinth.Items.Remove(this);
                YMCL.App.UiRoot.ViewModel.Download._modrinth.SelectedItem =
                    YMCL.App.UiRoot.ViewModel.Download._modrinth.Items.LastOrDefault();
            }
            else
            {
                YMCL.App.UiRoot.ViewModel.Download._modrinth.Items.Remove(this);
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        GC.Collect(2, GCCollectionMode.Aggressive, true);
    }
}